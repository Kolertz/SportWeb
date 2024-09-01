using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SportWeb.Models;
using SportWeb.Models.Entities;

namespace SportWeb.Services
{
    public interface IExerciseService
    {
        Task<Exercise?> GetExerciseAsync(int id, bool noTracking = false, string[]? includes = null);
    }
    public class ExerciseService(ApplicationContext db, ILogger<ExerciseService> logger) : IExerciseService
    {
        public async Task<Exercise?> GetExerciseAsync(int id, bool noTracking = false, string[]? includes = null)
        {
            IQueryable<Exercise> query = db.Exercises;
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }
            if (noTracking)
            {
                query = query.AsNoTracking();
            }
            var exercise = await query.FirstOrDefaultAsync(w => w.Id == id);
            logger.LogInformation("Get exercise: {exercise?.Name}", exercise?.Name);

            return exercise;
        }
    }
}
