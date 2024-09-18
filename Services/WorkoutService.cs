using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Caching.Memory;
using SportWeb.Migrations;
using SportWeb.Models;
using SportWeb.Models.Entities;
using System.Linq.Expressions;

namespace SportWeb.Services
{
    public interface IWorkoutRepository
    {
        Task<Workout?> GetWorkoutAsync(int id, bool noTracking = false, string[]? includes = null);
        (string, IQueryable<Workout>) GetUserWorkouts(int id, string username, bool isUserWorkouts);
    }
    public interface IWorkoutEditorService
    {
        Task AddSupersetAsync(Workout workout);
        Task<Workout> AddWorkoutAsync(string name, int authorId);
        Task AddExerciseAsync(Workout workout, Exercise exercise);
        Task RemoveExerciseAsync(int exerciseId, Workout workout);
        Task RemoveSupersetAsync(int supersetId, Workout workout);
        Task UpdateWorkoutPositions(ICollection<object> workoutItems, int workoutId);
        List<object> SortWorkoutItems(List<WorkoutExercise> exercises, List<Superset> supersets);
        Task UpdateWorkoutPositionsAsync(Workout workout, List<WorkoutPositionsModel> workoutPositions, bool isPublic, string description);
    }
    public interface IWorkoutCacheService
    {
        void RemoveWorkoutFromCache(int id);
        void SetWorkoutIncludesToCache(int id, HashSet<string>? missingIncludes, HashSet<string>? cachedIncludes);
        void SetWorkoutToCache(Workout? workout);
    }
    public class WorkoutService(ApplicationContext db, ILogger<WorkoutService> logger, IMemoryCache memoryCache) : IWorkoutRepository, IWorkoutEditorService, IWorkoutCacheService
    {
        public async Task<Workout?> GetWorkoutAsync(int id, bool noTracking = false, string[]? includes = null)
        {
            try
            {
                // Попробуем получить тренировку из кэша
                var workout = GetWorkoutFromCache(id);

                if (workout == null)
                {
                    // Если тренировки нет в кэше, загрузим её из базы данных
                    IQueryable<Workout> query = db.Workouts;

                    // Если указаны включения, добавляем их в запрос
                    if (includes is not null)
                    {
                        foreach (var include in includes)
                        {
                            query = query.Include(include);
                        }
                    }

                    // Выполняем запрос к базе данных с учетом параметра noTracking
                    workout = noTracking
                        ? await query.AsNoTracking().FirstOrDefaultAsync(w => w.Id == id)
                        : await query.FirstOrDefaultAsync(w => w.Id == id);

                }
                else if (includes is not null)
                {
                    HashSet<string> missingIncludes = GetMissingIncludes(workout, includes);
                    if (includes is not null && missingIncludes.Count > 0) // Если данные получены из кэша, но требуются дополнительные включения
                    {

                        // Получить недостающие данные и обновить пользователя в кэше
                        workout = await LoadMissingIncludesAsync(workout.Id, missingIncludes);
                        SetWorkoutToCache(workout);
                    }
                }
                
                return workout;
            }
            catch (Exception ex)
            {
                // Логируем исключение с дополнительной информацией
                logger.LogError(ex, "An error occurred while trying to get workout with ID {WorkoutId}", id);
                return null;
            }
        }

        // Метод для проверки наличия необходимых включений у пользователя
        private HashSet<string> GetMissingIncludes(Workout workout, string[] requiredIncludes)
        {
            var missingIncludes = new HashSet<string>();
            var cachedIncludes = GetWorkoutIncludesFromCache(workout.Id);

            if (cachedIncludes is not null)
            {
                foreach (var requiredInclude in requiredIncludes)
                {
                    if (!cachedIncludes.Any(x => x.StartsWith(requiredInclude)))
                    {
                        missingIncludes.Add(requiredInclude);
                    }
                }
            } else
            {
                missingIncludes = [.. requiredIncludes];
            }
            SetWorkoutIncludesToCache(workout.Id, missingIncludes, cachedIncludes);
            return missingIncludes;
        }

        public void SetWorkoutIncludesToCache(int id, HashSet<string>? missingIncludes, HashSet<string>? cachedIncludes)
        {
            cachedIncludes ??= [];
            missingIncludes ??= [];
            var UpdatedIncludes = new HashSet<string>(cachedIncludes);
            foreach (var cachedInclude in cachedIncludes)
            {
                if (missingIncludes.Any(x => x.StartsWith(cachedInclude)))
                {
                    UpdatedIncludes.Remove(cachedInclude);
                    break;
                }
            }

            memoryCache.Set(GetWorkoutIncludesCacheKey(id), missingIncludes.Union(cachedIncludes), new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(30) // Настройка времени жизни в кэше
            });
            logger.LogInformation($"Workout {id} includes were set to cache");
        }
        // Универсальный метод для загрузки недостающих данных
        private async Task<Workout?> LoadMissingIncludesAsync(int id, HashSet<string> includes)
        {
            var query = db.Workouts.AsQueryable();

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync(w => w.Id == id);
        }
        private HashSet<string> GetWorkoutIncludesFromCache(int id)
        {
            memoryCache.TryGetValue(GetWorkoutIncludesCacheKey(id), out HashSet<string>? cachedIncludes);
            return cachedIncludes ?? [];
        }
        public void SetWorkoutToCache(Workout? workout)
        {
            if (workout == null)
            {
                logger.LogWarning("Attempted to cache a null workout.");
                return;
            }

            memoryCache.Set(GetWorkoutCacheKey(workout.Id), workout, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(30) // Настройка времени жизни в кэше
            });
            logger.LogInformation($"Workout {workout.Name} was set to cache");
        }

        private Workout? GetWorkoutFromCache(int id)
        {
            if (memoryCache.TryGetValue(GetWorkoutCacheKey(id), out Workout? workout))
            {
                logger.LogInformation($"Workout with id {id} retrieved from cache.");
                return workout;
            }

            logger.LogInformation($"Workout with id {id} not found in cache.");
            return null;
        }

        public List<object> SortWorkoutItems(List<WorkoutExercise> exercises, List<Superset> supersets)
        {
            foreach (var superset in supersets)
            {
                superset.WorkoutExercises = [.. superset.WorkoutExercises.OrderBy(we => we.Position)];
            }
            var items = exercises.Cast<object>().Concat(supersets).Cast<object>().OrderBy(GetPosition).ToList();
            return items;
        }

        public async Task UpdateWorkoutPositions(ICollection<object> workoutItems, int workoutId)
        {
            var workout = await GetWorkoutAsync(workoutId);
            if (workout == null) { return; }

            var workoutExerciseDict = workout.WorkoutExercises.ToDictionary(x => x.ExerciseId);
            var supersetDict = workout.Supersets.ToDictionary(x => x.Id);

            // Обновление позиций
            foreach (var item in workoutItems)
            {
                switch (item)
                {
                    case WorkoutExercise workoutExercise when workoutExercise.Exercise is not null:
                        if (workoutExerciseDict.TryGetValue(workoutExercise.Exercise.Id, out var dbWorkoutExercise))
                        {
                            dbWorkoutExercise.Position = workoutExercise.Position;
                        }
                        break;

                    case Superset superset when superset.WorkoutExercises is not null:
                        if (supersetDict.TryGetValue(superset.Id, out var dbSuperset))
                        {
                            dbSuperset.Position = superset.Position;

                            // Обновление позиций внутри Superset
                            var dbWorkoutExercisesInSuperset = dbSuperset.WorkoutExercises.ToDictionary(x => x.ExerciseId);
                            foreach (var workoutExerciseInSuperset in superset.WorkoutExercises)
                            {
                                if (dbWorkoutExercisesInSuperset.TryGetValue(workoutExerciseInSuperset.ExerciseId, out var dbWorkoutExerciseInSuperset))
                                {
                                    dbWorkoutExerciseInSuperset.Position = workoutExerciseInSuperset.Position;
                                }
                            }
                        }
                        break;
                }
            }

            if (db.ChangeTracker.HasChanges())
            {
                await db.SaveChangesAsync();
                logger.LogInformation("Order saved successfully!");
            }
            else
            {
                logger.LogInformation("No changes were made to the order.");
            }
        }

        private int GetPosition(object item)
        {
            return item switch
            {
                WorkoutExercise we => we.Position,
                Superset s => s.Position,
                _ => throw new ArgumentException("The item is not a workout or superset")
            };
        }

        public async Task AddSupersetAsync(Workout workout)
        {
            var exerciseCount = workout.WorkoutExercises == null ? 0 : workout.WorkoutExercises.Count + workout.Supersets.Count;
            var newSuperset = new Superset
            {
                WorkoutId = workout.Id,
                Position = exerciseCount
            };

            workout.Supersets ??= [];
            workout.Supersets.Add(newSuperset);

            await db.SaveChangesAsync();
        }

        public async Task<Workout> AddWorkoutAsync(string name, int authorId)
        {
            Workout workout = new()
            {
                Name = name,
                AuthorId = authorId,
                IsPublic = true
            };
            await db.Workouts.AddAsync(workout);
            await db.SaveChangesAsync();

            return workout;
        }

        public async Task AddExerciseAsync(Workout workout, Exercise exercise)
        {
            var exerciseExists = await db.WorkoutExercises.AnyAsync(x => x.ExerciseId == exercise.Id && x.WorkoutId == workout.Id); // check if exercise exists
            var exerciseCount = workout.WorkoutExercises == null ? 0 : workout.WorkoutExercises.Count + workout.Supersets.Count;

            if (exercise is not null && exercise.State == ExerciseState.Approved && !exerciseExists)
            {
                var workoutExercise = new WorkoutExercise
                {
                    WorkoutId = workout.Id,
                    ExerciseId = exercise.Id,
                    Position = exerciseCount
                };
                workout.WorkoutExercises?.Add(workoutExercise);
                await db.SaveChangesAsync();
                logger.LogInformation("Exercise added to a workout successfully");
            }
            else
            {
                logger.LogWarning("Exercise not found or not approved");
            }
        }

        public async Task RemoveExerciseAsync(int exerciseId, Workout workout)
        {
            logger.LogInformation("Trying to remove an exercise from the workout...");
            var exerciseToRemove = workout.WorkoutExercises.SingleOrDefault(x => x.ExerciseId == exerciseId);
            if (exerciseToRemove is not null)
            {
                workout.WorkoutExercises.Remove(exerciseToRemove);
                await db.SaveChangesAsync();
                logger.LogInformation("Exercise removed from the workout successfully");
            }
            else
            {
                logger.LogWarning("Exercise with ID {ExerciseId} not found in workout", exerciseId);
            }
        }

        public async Task RemoveSupersetAsync(int supersetId, Workout workout)
        {
            logger.LogInformation("Trying to remove a superset from the workout...");
            var supersetToRemove = workout.Supersets.SingleOrDefault(x => x.Id == supersetId);
            if (supersetToRemove is not null)
            {
                db.RemoveRange(supersetToRemove.WorkoutExercises);
                workout.Supersets.Remove(supersetToRemove);
                await db.SaveChangesAsync();
                logger.LogInformation("Superset removed from the workout successfully");
            }
            else
            {
                logger.LogWarning("Superset with ID {SupersetId} not found in workout", supersetId);
            }
        }
        public async Task UpdateWorkoutPositionsAsync (Workout workout, List<WorkoutPositionsModel> workoutPositions, bool isPublic, string description)
        {
            var supersetsId = workout.Supersets.ToDictionary(x => x.Id);
            var workoutExercisesId = workout.WorkoutExercises.ToDictionary(x => x.ExerciseId);
            foreach (var position in workoutPositions)
            {
                switch (position)
                {
                    case { IsSuperset: true }:
                        supersetsId[position.Id].Position = position.Position;
                        break;

                    case { SupersetId: not null }:
                        workoutExercisesId[position.Id].Position = position.Position;
                        workoutExercisesId[position.Id].SupersetId = position.SupersetId;
                        break;

                    default:
                        workoutExercisesId[position.Id].Position = position.Position;
                        workoutExercisesId[position.Id].SupersetId = null;
                        break;
                }
            }
            workout.IsPublic = isPublic;
            workout.Description = description;
            if (db.ChangeTracker.HasChanges())
            {
                await db.SaveChangesAsync();
                logger.LogInformation("Workout positions updated successfully");
            }
            else
            {
                logger.LogWarning("Workout positions were not updated");
            }
        }

        public (string, IQueryable<Workout>) GetUserWorkouts(int id, string username, bool isUserWorkouts)
        {
            
            IQueryable<Workout> workouts = db.Workouts;

            string title;
            if (isUserWorkouts)
            {
                title = "My workouts";
                workouts = workouts.Where(x => x.AuthorId == id);
            }
            else
            {
                title = $"{username}'s workouts";
                workouts = workouts.Where(x => x.AuthorId == id && x.IsPublic);
            }

            return (title, workouts);
        }

        private static string GetWorkoutIncludesCacheKey(int id) => $"workout-{id}-includes";
        private static string GetWorkoutCacheKey(int id) => $"workout-{id}";
        public void RemoveWorkoutFromCache(int id)
        {
            memoryCache.Remove(GetWorkoutCacheKey(id));
        }
    }
}