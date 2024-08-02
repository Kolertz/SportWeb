﻿using SportWeb.Models;
using Microsoft.EntityFrameworkCore;
using SportWeb.Extensions;
using SportWeb.Models.Entities;
using System.Text.RegularExpressions;

namespace SportWeb.Services
{
    public interface IUserService
    {
        Task<User?> GetUserAsync<T>(T id);
        Task<User?> AddUserAcync(string? name, string email, string password);
        Task<bool> RemoveUserAsync<T>(T id);
        bool IsCurrentUser(int id);
        Task<bool> IsUserExistsByEmail(string email);
        int CastToInt<T>(T id);
        bool IsAdmin<T>(T id);
        
    }
    public class UserService : IUserService
    {
        readonly ILogger<UserService> logger;
        readonly ApplicationContext db;
        readonly IPasswordCryptor passwordCryptor;
        readonly IHttpContextAccessor httpContextAccessor;
        public UserService(ApplicationContext db, ILogger<UserService> logger, IPasswordCryptor passwordCryptor, IHttpContextAccessor httpContextAccessor)
        {
            this.db = db;
            this.logger = logger;
            this.passwordCryptor = passwordCryptor;
            this.httpContextAccessor = httpContextAccessor;
        }
        public async Task<User?> GetUserAsync<T>(T id)
        {
            try
            {
                if (id == null)
                {
                    logger.LogWarning("Null id passed to GetUserAsync");
                    return null;
                }

                int intId = CastToInt(id);

                if (IsCurrentUser(intId))
                {
                    logger.LogInformation("User is current user");
                    return await GetUserFromSession(intId);
                }
                var user = await db.Users.FirstOrDefaultAsync(u => u.Id == intId);
                return user;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while trying to get user");
                return null;
            }
        }
        public async Task<User?> GetUserFromSession(int id)
        {
            var context = httpContextAccessor.HttpContext;
            if (context != null && context.User.Identity != null && context.User.Identity.Name == id.ToString())
            {
                if (!context.Session.Keys.Contains("User"))
                {
                    logger.LogInformation("User is not in session");
                    await SetUserToSession(id);
                }

                return context.Session.Get<User>("User");
            }
            return null;
        }

        public bool IsCurrentUser(int id)
        {
            var context = httpContextAccessor.HttpContext;
            return context?.User?.Identity?.Name == id.ToString();
        }

        private async Task SetUserToSession(int id)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user != null)
            {
                SetUserToSession(user);
            }
        }

        private void SetUserToSession(User user)
        {
            var context = httpContextAccessor.HttpContext;
            if (context != null && user != null)
            {
                context.Session.Set("User", user);
                logger.LogInformation($"User {user.Name} was set to session");
            }
        }

        public async Task<User?> AddUserAcync(string? name, string email, string password)
        {
            try
            {
                User newUser = new User { Name = name, Email = email, Password = passwordCryptor.Hash(password)};
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

                User? user = GetUserAsync(id).Result;
                if (user == null)
                {
                    logger.LogWarning("User not found");
                    return false;
                }
                db.Users.Remove(user);
                await db.SaveChangesAsync();
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
        
    }
}

