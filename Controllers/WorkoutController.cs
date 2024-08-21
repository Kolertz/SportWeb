using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SportWeb.Extensions;
using SportWeb.Migrations;
using SportWeb.Models;
using SportWeb.Models.Entities;
using SportWeb.Services;

namespace SportWeb.Controllers
{
    public class WorkoutController(
        ApplicationContext db,
        ILogger<WorkoutController> logger,
        IUserService userService,
        IPaginationService paginationService,
        IWorkoutService workoutService) : Controller
    {
        [Route("Profile/{id}/Workouts")]
        public async Task<IActionResult> UserWorkouts(int id, int page = 1, int pageSize = 5, string? username = "???")
        {
            var isUserWorkouts = userService.IsCurrentUser(id);
            IQueryable<Workout> workouts = db.Workouts.Where(x => x.AuthorId == id || x.IsPublic);
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
            var workout = await workoutService.GetWorkoutAsync(id);

            if (workout == null)
            {
                return NotFound();
            }
            if (!workout.IsPublic && !userService.IsCurrentUser(workout.AuthorId))
            {
                return Forbid();
            }
            foreach (var superset in workout.Supersets)
            {
                superset.WorkoutExercises = [.. superset.WorkoutExercises.OrderBy(we => we.Position)];
            }
            var workoutItems = workoutService.SortWorkoutItems([.. workout.WorkoutExercises], [.. workout.Supersets]);
            ViewBag.WorkoutItems = workoutItems;
            return View(workout);
        }
        
        [Authorize]
        public IActionResult Create() => View();

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(string name)
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            Workout workout = new()
            { 
                Name = name, 
                AuthorId = int.Parse(User.Identity?.Name!),
                IsPublic = true
            };
            await db.Workouts.AddAsync(workout);
            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Save), new { workout, workout.Id });
        }

        [HttpPost]
        public async Task<IActionResult> AddExercise(int workoutId, int? exerciseId)
        {
            if (exerciseId != null)
            {
                var workout = await workoutService.GetWorkoutAsync(workoutId);
                if (workout == null)
                {
                    return NotFound();
                }
                if (!userService.IsCurrentUser(workout.AuthorId))
                {
                    return Forbid();
                }
                logger.LogInformation("Trying to add exercise to a workout...");
                var exercise = await db.Exercises.AsNoTracking().FirstOrDefaultAsync(x => x.Id == exerciseId);
                var exerciseExists = await db.WorkoutExercises.AnyAsync(x => x.ExerciseId == exerciseId && x.WorkoutId == workoutId); // check if exercise exists
                var exerciseCount = workout.WorkoutExercises == null ? 0 : workout.WorkoutExercises.Count + workout.Supersets.Count;

                if (exercise != null && exercise.State == ExerciseState.Approved && !exerciseExists)
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

            return RedirectToAction(nameof(Save), new { workoutId });
        }

        [HttpPost]
        public async Task<IActionResult> AddSuperset(int workoutId)
        {
            // Получаем тренировку по workoutId
            var workout = await workoutService.GetWorkoutAsync(workoutId);
            if (workout == null)
            {
                return NotFound();
            }
            if (!userService.IsCurrentUser(workout.AuthorId))
            {
                return Forbid();
            }

            logger.LogInformation("Trying to add a superset to a workout...");
            var exerciseCount = workout.WorkoutExercises == null ? 0 : workout.WorkoutExercises.Count + workout.Supersets.Count;

            var newSuperset = new Superset
            {
                WorkoutId = workoutId,
                Position = exerciseCount
            };

            workout.Supersets ??= [];
            workout.Supersets.Add(newSuperset);

            await db.SaveChangesAsync();

            logger.LogInformation("Superset added to the workout successfully");

            return RedirectToAction(nameof(Save), new { id = workoutId });
        }
        public async Task<IActionResult> RemoveExercise(int workoutId, int exerciseId)
        {
            var workout = await workoutService.GetWorkoutAsync(workoutId);
            if (workout == null)
            {
                return NotFound();
            }
            if (!userService.IsCurrentUser(workout.AuthorId))
            {
                return Forbid();
            }

            logger.LogInformation("Trying to remove an exercise from the workout...");

            var exerciseToRemove = workout.WorkoutExercises.SingleOrDefault(x => x.ExerciseId == exerciseId);
            if (exerciseToRemove != null)
            {
                workout.WorkoutExercises.Remove(exerciseToRemove);
                await db.SaveChangesAsync();
                logger.LogInformation("Exercise removed from the workout successfully");
            }
            else
            {
                logger.LogWarning("Exercise with ID {ExerciseId} not found in workout", exerciseId);
            }

            return RedirectToAction(nameof(Save), new { id = workoutId });
        }

        public async Task<IActionResult> RemoveSuperset(int workoutId, int supersetId)
        {
            var workout = await workoutService.GetWorkoutAsync(workoutId);
            if (workout == null)
            {
                return NotFound();
            }
            if (!userService.IsCurrentUser(workout.AuthorId))
            {
                return Forbid();
            }

            logger.LogInformation("Trying to remove a superset from the workout...");

            var supersetToRemove = workout.Supersets.SingleOrDefault(x => x.Id == supersetId);
            if (supersetToRemove != null)
            {
                workout.Supersets.Remove(supersetToRemove);
                await db.SaveChangesAsync();
                logger.LogInformation("Superset removed from the workout successfully");
            }
            else
            {
                logger.LogWarning("Superset with ID {SupersetId} not found in workout", supersetId);
            }

            return RedirectToAction(nameof(Save), new { id = workoutId });
        }


        //[Authorize]
        [Route("Workout/Save/{workoutId}")]
        public async Task<IActionResult> Save(int workoutId)
        {
            logger.LogInformation("We are on the GET method");
            var workout = await workoutService.GetWorkoutAsync(workoutId);

            if (workout == null)
            {
                return NotFound();
            }
            if (!userService.IsCurrentUser(workout.AuthorId))
            {
                return Forbid();
            }
            HttpContext.Session.Set("SelectedWorkout", workout);

            var workoutItems = workoutService.SortWorkoutItems([.. workout.WorkoutExercises], [.. workout.Supersets]);

            WorkoutViewModel model = new ()
            {
                Id = workout.Id,
                IsPublic = workout.IsPublic,
                Description = workout.Description,
                WorkoutItems = workoutItems
            };
            return View(model);
        }

        //[Authorize]
        [HttpPost]
        public async Task<IActionResult> SaveOrder(int workoutId, List<WorkoutPositionsModel> workoutPositions)
        {
            logger.LogError("Saving order for workout with ID {WorkoutId}. Received {Count} workout positions.", workoutId, workoutPositions.Count);
            foreach (var position in workoutPositions)
            {
                logger.LogError("Workout Position Model Details: Id = {Id}, Position = {Position}, IsSuperset = {IsSuperset}, SupersetId = {SupersetId}",
                    position.Id, position.Position, position.IsSuperset, position.SupersetId.HasValue ? position.SupersetId.Value.ToString() : "null");
            }
            var workout = await workoutService.GetWorkoutAsync(workoutId);
            if (workout == null)
            {
                return NotFound();
            }
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
                        break;
                }
            }
            if (db.ChangeTracker.HasChanges())
            {
                await db.SaveChangesAsync();
                logger.LogInformation("Workout positions updated successfully");
            } else
            {
                logger.LogWarning("Workout positions were not updated");
            }
            HttpContext.Session.Remove("SelectedWorkout");
            return RedirectToAction(nameof(Save), new { id = workoutId });
        }
        /*
        if (model == null || !ModelState.IsValid)
        {
            var errors = ModelState
            .Where(ms => ms.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
            );

                return BadRequest(new
                {
                    Message = "Invalid workout data.",
                    Errors = errors
                });
        }

        var workout = await workoutService.GetWorkoutAsync(model.Id);
        if (workout == null)
        {
            return NotFound();
        }

        try
        {
            workout.IsPublic = model.IsPublic;

            // Создание словарей для быстрого доступа
            var workoutExerciseDict = workout.WorkoutExercises.ToDictionary(x => x.ExerciseId);
            var supersetDict = workout.Supersets.ToDictionary(x => x.Id);

            await workoutService.UpdateWorkoutPositions(model.WorkoutItems, model.Id);
            logger.LogInformation("Workout positions updated successfully");
            HttpContext.Session.Remove("SelectedWorkout");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while saving workout order.");
            return StatusCode(500, "Internal server error");
        }

        return RedirectToAction(nameof(Details), new { id = workoutId });
        }
        */
    }
}

