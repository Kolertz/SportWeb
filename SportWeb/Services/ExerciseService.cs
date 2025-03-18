using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SportWeb.Models;
using SportWeb.Models.Entities;

namespace SportWeb.Services
{
    public interface IExerciseService
    {
        Task<Exercise?> GetExerciseAsync(int id, bool noTracking = false, string[]? includes = null);
        IQueryable<Exercise> GetExercisesAsync(int? muscle, int? movement, int? tag, int? equipment, string? name);
        List<IndexExerciseViewModel> GetExerciseViewModels(List<Exercise> items);
        Task AddExerciseAsync(bool isAdmin, Exercise exercise, EditExerciseViewModel model);
    }
    public interface IExerciseCacheService
    {
        Task RemoveExerciseFromCacheAsync(int id);
        void SetExerciseToCache(Exercise? exercise);
    }
    public class ExerciseService(
        ApplicationContext db,
        ILogger<ExerciseService> logger,
        IMemoryCache memoryCache,
        IUserCacheService userCacheService,
        IOutputCacheStore outputCacheStore,
        IFileService fileService,
        IPictureService pictureService,
        IHttpContextAccessor httpContextAccessor) : IExerciseService, IExerciseCacheService
    {
        public List<IndexExerciseViewModel> GetExerciseViewModels(List<Exercise> items)
        {
            var result = items.Select(x => new IndexExerciseViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                AuthorId = x.AuthorId,
                Username = x.User is not null ? x.User.Name : "Unknown",
                IsFavourite = x.UsersWhoFavourited.Any(u => u.Id == userCacheService.GetCurrentUserId())
            }).ToList();

            return result;
        }
        public IQueryable<Exercise> GetExercisesAsync(int? muscle, int? movement, int? tag, int? equipment, string? name)
        {
            IQueryable<Exercise> exercises = db.Exercises.Where(x => x.State == ExerciseState.Approved).Include(u => u.User).Include(c => c.Categories).Include(u => u.UsersWhoFavourited).OrderBy(x => x.Name);

            if (muscle is not null && muscle != 0)
            {
                exercises = exercises.Where(e => e.Categories!.Any(c => c.Id == muscle));
            }
            if (movement is not null && movement != 0)
            {
                exercises = exercises.Where(e => e.Categories!.Any(c => c.Id == movement));
            }
            if (tag is not null && tag != 0)
            {
                exercises = exercises.Where(e => e.Categories!.Any(c => c.Id == tag));
            }
            if (equipment is not null && equipment != 0)
            {
                exercises = exercises.Where(e => e.Categories!.Any(c => c.Id == equipment));
            }
            if (name is not null && name.Length > 1)
            {
                exercises = exercises.Where(p => p.Name.Contains(name));
            }

            return exercises;
        }
        public async Task<Exercise?> GetExerciseAsync(int id, bool noTracking = false, string[]? includes = null)
        {
            try
            {
                // Попробуем получить упражнение из кэша
                var exercise = GetExerciseFromCache(id);

                if (exercise == null)
                {
                    // Если упражнения нет в кэше, загрузим его из базы данных
                    IQueryable<Exercise> query = db.Exercises;

                    if (includes is not null && includes.Length != 0)
                    {
                        foreach (var include in includes)
                        {
                            query = query.Include(include);
                        }
                    }

                    exercise = noTracking
                        ? await query.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id)
                        : await query.FirstOrDefaultAsync(e => e.Id == id);
                }
                else if (includes is not null && includes.Length != 0) // Если требуются дополнительные данные (включения)
                {
                    foreach (var include in includes)
                    {
                        var entry = db.Entry(exercise);
                        if (!entry.References.Any(r => r.Metadata.Name == include) ||
                            !entry.Collections.Any(c => c.Metadata.Name == include))
                        {
                            // Если включения не загружены, загрузим их из базы данных
                            await entry.Reference(include).LoadAsync();
                            await entry.Collection(include).LoadAsync();
                        }
                    }
                }

                // Сохраняем упражнение в кэше, если оно не null
                if (exercise is not null)
                {
                    SetExerciseToCache(exercise);
                }

                return exercise;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while trying to get exercise");
                return null;
            }
        }

        public void SetExerciseToCache(Exercise? exercise)
        {
            if (exercise == null)
            {
                logger.LogWarning("Attempted to cache a null exercise.");
                return;
            }

            memoryCache.Set(GetExerciseCacheKey(exercise.Id), exercise, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(30) // Настройка времени жизни в кэше
            });
            logger.LogInformation($"Exercise {exercise.Name} was set to cache");
        }

        private Exercise? GetExerciseFromCache(int id)
        {
            if (memoryCache.TryGetValue(GetExerciseCacheKey(id), out Exercise? exercise))
            {
                logger.LogInformation($"Exercise with id {id} retrieved from cache.");
                return exercise;
            }

            logger.LogInformation($"Exercise with id {id} not found in cache.");
            return null;
        }
        private static string GetExerciseCacheKey(int id) => $"exercise-{id}";
        public async Task RemoveExerciseFromCacheAsync(int id)
        {
            memoryCache.Remove(GetExerciseCacheKey(id));
            await outputCacheStore.EvictByTagAsync("ExerciseList", new CancellationToken());
        }
        public async Task AddExerciseAsync(bool isAdmin, Exercise exercise, EditExerciseViewModel model)
        {
            if (!isAdmin)
            {
                exercise.State = ExerciseState.Pending;
                logger.LogInformation("Exercise state changed to pending");
            }
            else
            {
                exercise.State = ExerciseState.Approved;
                logger.LogInformation("Exercise state changed to approved");
            }
            exercise.AuthorId = int.Parse(httpContextAccessor.HttpContext?.User.Identity!.Name!);
            exercise.Categories = (List<Category>?)model.SelectedCategories;

            var fileUpload = model.FileUpload;
            if (fileUpload is not null && fileUpload.Length > 0)
            {
                var filePath = pictureService.GetPicturePath(pictureService.NewPictureName(exercise.PictureUrl, exercise.Id));
                await fileService.UploadFileToServer(fileUpload, filePath);
            }

            await db.Exercises.AddAsync(exercise);
            await db.SaveChangesAsync();
        }
    }
}