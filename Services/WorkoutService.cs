using Microsoft.EntityFrameworkCore;
using SportWeb.Extensions;
using SportWeb.Models;
using SportWeb.Models.Entities;
using System.Linq;

namespace SportWeb.Services
{
    public interface IWorkoutService
    {
        List<object> SortWorkoutItems(List<WorkoutExercise> exercises, List<Superset> supersets);
        Task<Workout?> GetWorkoutAsync(int id, bool noTracking = false);
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
            var items = exercises.Cast<object>().Concat(supersets).Cast<object>().OrderBy(GetPosition).ToList();
            return items;
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
