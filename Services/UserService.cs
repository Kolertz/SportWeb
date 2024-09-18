using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SportWeb.Extensions;
using SportWeb.Models;
using SportWeb.Models.Entities;

namespace SportWeb.Services
{
    public interface IUserRepository
    {
        Task<User?> GetUserAsync<T>(T id, bool noTracking = false, string[]? includes = null);
        Task<User?> AddUserAsync(string? name, string email, string password);
        Task<bool> RemoveUserAsync<T>(T id);
        Task<bool> IsUserExistsByEmail(string email);
        Task<string> GetUserNameAsync(int id);
    }

    public interface IUserCacheService
    {
        void SetUserToCache(User? user);
        bool IsCurrentUser(int? id);
        Task RemoveUserFromCacheAsync(int id);
        int? GetCurrentUserId();
    }

    public interface IUserRoleService
    {
        bool IsAdmin<T>(T id);
    }

    public class UserService(ApplicationContext db, ILogger<UserService> logger, IPasswordCryptorService passwordCryptor, IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache, IOutputCacheStore outputCacheStore) : IUserRepository, IUserCacheService, IUserRoleService
    {
        public async Task<User?> GetUserAsync<T>(T id, bool noTracking = false, string[]? includes = null)
        {
            try
            {
                if (id == null)
                {
                    logger.LogWarning("Null id passed to GetUserAsync");
                    return null;
                }

                int intId = CastToInt(id);
                IQueryable<User> query = db.Users;
                if (noTracking)
                {
                    query = query.AsNoTracking();
                }
                if (intId == 0)
                {
                    logger.LogInformation("Email passed to GetUserAsync");
                    return await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == id.ToString());
                }
                var user = GetUserFromCache(intId);
                if (user == null)
                {
                    if (includes is not null && includes.Length != 0)
                    {
                        foreach (var include in includes)
                        {
                            query = query.Include(include);
                        }
                    }
                    
                user = await query.FirstOrDefaultAsync(u => u.Id == intId);
                } else if (includes is not null && includes.Length != 0) // Если требуются дополнительные данные (включения)
                {
                    foreach (var include in includes)
                    {
                        // Проверим, загружены ли включения в закэшированного пользователя
                        var entry = db.Entry(user);
                        if (!entry.References.Any(r => r.Metadata.Name == include) ||
                            !entry.Collections.Any(c => c.Metadata.Name == include))
                        {
                            // Если включения не загружены, загрузим их из базы данных
                            await entry.Reference(include).LoadAsync();
                            await entry.Collection(include).LoadAsync();
                        }
                    }
                }
                // Обновляем данные в кэше с новыми включениями
                SetUserToCache(user);
                return user;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while trying to get user");
                return null;
            }
        }

        private User? GetUserFromCache(int id)
        {
            if (memoryCache.TryGetValue(GetUserCacheKey(id), out User? user))
            {
                logger.LogInformation($"User with id {id} retrieved from cache.");
                return user;
            }

            logger.LogInformation($"User with id {id} not found in cache. Returning null.");
            return null;
        }

        public void SetUserToCache(User? user)
        {
            if (user == null)
            {
                logger.LogWarning("Null user passed to SetUserToCache");
                return;
            }
            memoryCache.Set(GetUserCacheKey(user.Id), user, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(30) // Настройка времени жизни в кэше
            });
            logger.LogInformation($"User {user.Name} was set to cache");
        }

        public bool IsCurrentUser(int? id)
        {
            var userId = GetCurrentUserId();
            return userId.ToString() == id.ToString();
        }

        public async Task<User?> AddUserAsync(string? name, string email, string password)
        {
            try
            {
                User newUser = new() { Name = name, Email = email, Password = passwordCryptor.Hash(password) };
                await db.Users.AddAsync(newUser);
                await db.SaveChangesAsync();

                logger.LogInformation($"User {newUser.Name} was created");
                return newUser;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error {ex} when trying to create user");
            }
            return null;
        }

        public async Task<bool> RemoveUserAsync<T>(T id)
        {
            try
            {
                if (id == null)
                {
                    logger.LogWarning("Null id passed to GetUserAsync");
                    return false;
                }

                User? user = await GetUserAsync(id);
                if (user == null)
                {
                    logger.LogWarning("User not found");
                    return false;
                }

                db.Users.Remove(user);
                await db.SaveChangesAsync();
                RemoveUserFromCacheAsync(user.Id);
                logger.LogInformation($"User {user.Name} was removed");

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error {ex} when trying to remove user");
                return false;
            }
        }

        public bool IsAdmin<T>(T id)
        {
            int intId = CastToInt(id);
            if (intId == 1)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> IsUserExistsByEmail(string email)
        {
            return await db.Users.AnyAsync(u => u.Email == email);
        }

        public int CastToInt<T>(T id)
        {
            int intId = 0;
            if (id is int)
            {
                intId = (int)(object)id; // Safe cast from object to int
            }
            else if (id is string strId && int.TryParse(strId, out int parsedId))
            {
                intId = parsedId;
            }
            return intId;
        }

        public int? GetCurrentUserId()
        {
            var context = httpContextAccessor.HttpContext;
            if (context is not null && context.User.Identity is not null && context.User.Identity.Name is not null)
            {
                return int.Parse(context.User.Identity.Name);
            }
            return null;
        }

        public async Task<string> GetUserNameAsync(int id)
        {
            var user = await GetUserAsync(id, true);
            var username = user?.Name ?? "Anonymous";

            return username;
        }
        private static string GetUserCacheKey(int userId) => $"user-{userId}";
        public async Task RemoveUserFromCacheAsync(int id)
        {
            memoryCache.Remove(GetUserCacheKey(id));
            await outputCacheStore.EvictByTagAsync("UserData", new CancellationToken());
        }
    }
}