using Microsoft.EntityFrameworkCore;
using SportWeb.Models;
using SportWeb.Models.Entities;

namespace SportWeb.Services
{
    public interface IWorkoutService
    {
        List<object> SortWorkoutItems(List<WorkoutExercise> exercises, List<Superset> supersets);

        Task<Workout?> GetWorkoutAsync(int id, bool noTracking = false);

        Task UpdateWorkoutPositions(ICollection<object> workoutItems, int workoutId);
    }

    public class WorkoutService(IHttpContextAccessor httpContextAccessor, ApplicationContext db, ILogger<WorkoutService> logger) : IWorkoutService
    {
        public async Task<Workout?> GetWorkoutAsync(int id, bool noTracking = false)
        {
            var context = httpContextAccessor.HttpContext;
            /*
            Workout? workout = null;
            if (context != null && context.Session.Keys.Contains("SelectedWorkout"))
            {
                workout = context.Session.Get<Workout>("SelectedWorkout");
                logger.LogInformation("Workout is in session");
                if (workout != null && workout.Id == id)
                {
                    logger.LogInformation("Session's workout is correct");
                    return workout;
                }
            }
            */
            logger.LogInformation("Workout is not in session");
            IQueryable<Workout> query = db.Workouts
            .Include(x => x.Supersets)
            .Include(x => x.WorkoutExercises)
                .ThenInclude(we => we.Exercise)
                .ThenInclude(ex => ex.User);
            if (noTracking)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(w => w.Id == id);
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
                    case WorkoutExercise workoutExercise when workoutExercise.Exercise != null:
                        if (workoutExerciseDict.TryGetValue(workoutExercise.Exercise.Id, out var dbWorkoutExercise))
                        {
                            dbWorkoutExercise.Position = workoutExercise.Position;
                        }
                        break;

                    case Superset superset when superset.WorkoutExercises != null:
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
    }
}