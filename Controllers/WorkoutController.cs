using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportWeb.Extensions;
using SportWeb.Models;
using SportWeb.Models.Entities;
using SportWeb.Services;

namespace SportWeb.Controllers
{
    public class WorkoutController(
        IUserCacheService userCacheService,
        ILogger<WorkoutController> logger,
        IPaginationService paginationService,
        IExerciseService exerciseService,
        IWorkoutRepository workoutRepository,
        IWorkoutEditorService workoutEditorService,
        IUserRepository userRepository) : Controller
    {
        [Route("profile/{id}/workouts")]
        public async Task<IActionResult> UserWorkouts(int id, int page = 1, int pageSize = 5)
        {
            var isUserWorkouts = userCacheService.IsCurrentUser(id);
            var username = await userRepository.GetUserNameAsync(id);
            (var title, var workouts) = workoutRepository.GetUserWorkouts(id, username, isUserWorkouts);
            ViewBag.Title = title;

            (var items, var model) = await paginationService.GetPaginatedResultAsync(workouts, page, pageSize);
            ViewBag.Id = id;
            ViewBag.Workouts = items;
            ViewBag.IsUserWorkouts = isUserWorkouts;
            ViewBag.Username = username;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var workout = await workoutRepository.GetWorkoutAsync(id, false, ["Supersets", "WorkoutExercises.Exercise.User"]);
            if (workout == null)
            {
                return NotFound();
            }
            if (!workout.IsPublic && !userCacheService.IsCurrentUser(workout.AuthorId))
            {
                return Forbid();
            }
            foreach (var superset in workout.Supersets)
            {
                superset.WorkoutExercises = [.. superset.WorkoutExercises.OrderBy(we => we.Position)];
            }
            var workoutItems = workoutEditorService.SortWorkoutItems([.. workout.WorkoutExercises], [.. workout.Supersets]);

            WorkoutViewModel model = new()
            {
                Id = workout.Id,
                IsPublic = workout.IsPublic,
                Description = workout.Description,
                WorkoutItems = workoutItems,
                AuthorId = workout.AuthorId
            };
            return View(model);
        }

        [Authorize]
        public IActionResult Create() => View();

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(string name)
        {
            var user = User.Identity!;
            if (!user.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            var authorId = int.Parse(user.Name!);
            var workout = await workoutEditorService.AddWorkoutAsync(name, authorId);

            return RedirectToAction(nameof(Save), new { workout, workout.Id });
        }

        [HttpPost]
        public async Task<IActionResult> AddExercise(int workoutId, int? exerciseId)
        {
            if (exerciseId is not null)
            {
                var workout = await workoutRepository.GetWorkoutAsync(workoutId);
                if (workout == null)
                {
                    return NotFound();
                }
                if (!userCacheService.IsCurrentUser(workout.AuthorId))
                {
                    return Forbid();
                }
                logger.LogInformation("Trying to add exercise to a workout...");
                //var exercise = await db.Exercises.AsNoTracking().FirstOrDefaultAsync(x => x.Id == exerciseId);
                var exercise = await exerciseService.GetExerciseAsync(exerciseId.Value);
                await workoutEditorService.AddExerciseAsync(workout, exercise!);
            }

            return RedirectToAction(nameof(Save), new { workoutId });
        }

        [HttpPost]
        public async Task<IActionResult> AddSuperset(int workoutId)
        {
            // Получаем тренировку по workoutId
            var workout = await workoutRepository.GetWorkoutAsync(workoutId);
            if (workout == null)
            {
                return NotFound();
            }
            if (!userCacheService.IsCurrentUser(workout.AuthorId))
            {
                return Forbid();
            }

            logger.LogInformation("Trying to add a superset to a workout...");
            await workoutEditorService.AddSupersetAsync(workout);

            return RedirectToAction(nameof(Save), new { id = workoutId });
        }

        public async Task<IActionResult> RemoveExercise(int workoutId, int exerciseId)
        {
            var workout = await workoutRepository.GetWorkoutAsync(workoutId);
            if (workout == null)
            {
                return NotFound();
            }
            if (!userCacheService.IsCurrentUser(workout.AuthorId))
            {
                return Forbid();
            }

            await workoutEditorService.RemoveExerciseAsync(exerciseId, workout);
            
            return RedirectToAction(nameof(Save), new { id = workoutId });
        }

        public async Task<IActionResult> RemoveSuperset(int workoutId, int supersetId)
        {
            var workout = await workoutRepository.GetWorkoutAsync(workoutId);
            if (workout == null)
            {
                return NotFound();
            }
            if (!userCacheService.IsCurrentUser(workout.AuthorId))
            {
                return Forbid();
            }

            await workoutEditorService.RemoveSupersetAsync(supersetId, workout);

            return RedirectToAction(nameof(Save), new { id = workoutId });
        }

        //[Authorize]
        [Route("workout/save/{workoutId}")]
        public async Task<IActionResult> Save(int workoutId)
        {
            logger.LogInformation("We are on the GET method");
            var workout = await workoutRepository.GetWorkoutAsync(workoutId);

            if (workout == null)
            {
                return NotFound();
            }
            if (!userCacheService.IsCurrentUser(workout.AuthorId))
            {
                return Forbid();
            }
            HttpContext.Session.Set("SelectedWorkout", workout);

            var workoutItems = workoutEditorService.SortWorkoutItems([.. workout.WorkoutExercises], [.. workout.Supersets]);

            WorkoutViewModel model = new()
            {
                Id = workout.Id,
                IsPublic = workout.IsPublic,
                Description = workout.Description,
                WorkoutItems = workoutItems,
                AuthorId = workout.AuthorId
            };
            return View(model);
        }

        //[Authorize]
        [HttpPost]
        public async Task<IActionResult> SaveOrder(int workoutId, List<WorkoutPositionsModel> workoutPositions, bool isPublic, string description)
        {
            Log.VariableLog(logger, nameof(isPublic), isPublic);
            Log.VariableLog(logger, nameof(description), description);
            logger.LogInformation($"IsPublic is {isPublic}");
            logger.LogInformation("Saving order for workout with ID {WorkoutId}. Received {Count} workout positions.", workoutId, workoutPositions.Count);

            foreach (var position in workoutPositions)
            {
                logger.LogInformation("Workout Position Model Details: Id = {Id}, Position = {Position}, IsSuperset = {IsSuperset}, SupersetId = {SupersetId}",
                    position.Id, position.Position, position.IsSuperset, position.SupersetId.HasValue ? position.SupersetId.Value.ToString() : "null");
            }
            var workout = await workoutRepository.GetWorkoutAsync(workoutId);
            if (workout == null)
            {
                return NotFound();
            }
            await workoutEditorService.UpdateWorkoutPositionsAsync(workout, workoutPositions, isPublic, description);
            HttpContext.Session.Remove("SelectedWorkout");
            return RedirectToAction(nameof(Save), new { id = workoutId });
        }
    }
}