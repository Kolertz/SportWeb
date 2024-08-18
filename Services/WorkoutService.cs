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
        Task<Workout?> GetWorkoutAsync(int id);
    }
    public class WorkoutService(IHttpContextAccessor httpContextAccessor, ApplicationContext db) : IWorkoutService
    {
        public async Task<Workout?> GetWorkoutAsync(int id)
        {
            var context = httpContextAccessor.HttpContext;
            Workout? workout = null;
            if (context != null && context.Session.Keys.Contains("SelectedWorkout"))
            {
                workout = context.Session.Get<Workout>("SelectedWorkout");
                if (workout?.Id == id)
                {  
                    return workout; 
                }
            }
            workout = await db.Workouts
            .Include(x => x.Supersets)
            .Include(x => x.WorkoutExercises)
                .ThenInclude(we => we.Exercise)
                .ThenInclude(ex => ex.User)
            .FirstOrDefaultAsync(x => x.Id == id);

            return workout;
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
