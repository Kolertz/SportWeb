using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportWeb.Extensions;
using SportWeb.Migrations;
using SportWeb.Models;
using SportWeb.Models.Entities;
using SportWeb.Services;

namespace SportWeb.Controllers
{
    public class WorkoutController(
        ApplicationContext db,
        ILogger<ControllerBase> logger,
        IUserService userService,
        IPaginationService paginationService,
        IWorkoutService workoutService) : Controller
    {
        [Route("Profile/{id}/Workouts")]
        public async Task<IActionResult> UserWorkouts(int id, int page = 1, int pageSize = 5, string? username = "???")
        {
            var isUserWorkouts = userService.IsCurrentUser(id);
            IQueryable<Workout> workouts = db.Workouts.Where(x => x.AuthorId == id && x.IsPublic);
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
        //[Authorize]
        [Route("Workout/Save/{workoutId}")]
        public async Task<IActionResult> Save(int workoutId, bool? IsSupersetAdded = false)
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
            
            if (IsSupersetAdded == true)
            {
                var exerciseCount = workout.WorkoutExercises == null ? 0 : workout.WorkoutExercises.Count + workout.Supersets.Count;
                var superset = new Superset
                {
                    WorkoutId = workout.Id,
                    Position = exerciseCount
                };
                workout.Supersets.Add(superset);
            }

            if (db.ChangeTracker.HasChanges())
            {
                await db.SaveChangesAsync();
            }
            HttpContext.Session.Set("SelectedWorkout", workout);
            foreach (var superset in workout.Supersets)
            {
                superset.WorkoutExercises.OrderBy(we => we.Position);
            }
            var workoutItems = workoutService.SortWorkoutItems([.. workout.WorkoutExercises], [.. workout.Supersets]);
            ViewBag.WorkoutItems = workoutItems;
            //ViewBag.Exercises = await db.WorkoutExercises.Where(x => x.WorkoutId == workoutId).Select(x => x.Exercise).ToListAsync();
            //var exercises = workout.WorkoutExercises?.OrderBy(x => x.Position).Select(we => we.Exercise).ToList();
            return View(workout);
        }
        /*
        //[Authorize]
        [HttpPost]
        public async Task<IActionResult> Save(Workout workout)
        {
            logger.LogInformation("We are on the POST method");
            if (!ModelState.IsValid)
            {
                return View(workout);
            }

            db.Update(workout);
            await db.SaveChangesAsync();
            HttpContext.Session.Remove("SelectedWorkout");
            return RedirectToAction(nameof(Details), new { id = workout.Id, workout });
        }
        */
    }
}
