using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using SportWeb.Extensions;
using SportWeb.Filters;
using SportWeb.Models;
using SportWeb.Models.Entities;
using SportWeb.Services;
using System.Collections.Generic;

namespace SportWeb.Controllers
{
    public class ExerciseController(
        ApplicationContext db,
        IExerciseService exerciseService,
        ILogger<ExerciseController> logger,
        IUserRepository userRepository,
        IUserCacheService userCacheService,
        IFileService fileService,
        IPictureService pictureService,
        IPaginationService paginationService,
        IAuthorizationService authorizationService,
        ICategoryService categoryService,
        IExerciseCacheService exerciseCacheService,
        IFileUploadFacadeService fileUploadFacadeService) : Controller
    {
        [HttpGet]
        [OutputCache(PolicyName = "NoCache")]
        public async Task<IActionResult> Index(int? muscle, int? movement, int? tag, int? equipment, string? name, int page = 1, int pageSize = 5)
        {
            var exercises = exerciseService.GetExercisesAsync(muscle, movement, tag, equipment, name);

            (var items, var paginationModel) = await paginationService.GetPaginatedResultAsync(exercises, page, pageSize);

            var result = exerciseService.GetExerciseViewModels(items);

            var (movements, tags, equipments, muscles) = await categoryService.GetCategoryFiltersAsync();

            ViewBag.SelectedWorkout = HttpContext.Session.Get<Workout>("SelectedWorkout");

            var request = HttpContext.Request;
            string returnUrl = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";

            var exerciseFilterModel = new ExerciseFilterModel
            {
                Movements = new SelectList(movements, "Id", "Name", movement),
                Tags = new SelectList(tags, "Id", "Name", tag),
                Equipments = new SelectList(equipments, "Id", "Name", equipment),
                Muscles = new SelectList(muscles, "Id", "Name", muscle),
                Name = name
            };
            var model = new IndexExercisesViewModel
            {
                FilterModel = exerciseFilterModel,
                PaginationModel = paginationModel,
                Exercises = result,
                ReturnUrl = returnUrl
            };
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            //var exercise = await db.Exercises.Include(x => x.User).Include(x => x.Categories).Include(x => x.UsersWhoFavourited).FirstOrDefaultAsync(x => x.Id == id);
            var exercise = await exerciseService.GetExerciseAsync(id, includes: ["User", "Categories", "UsersWhoFavourited"]);
            if (exercise == null)
            {
                return NotFound();
            }

            var request = HttpContext.Request;
            string returnUrl = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";

            var model = new ExerciseDetailsViewModel
            {
                Id = exercise.Id,
                Description = exercise.Description,
                Name = exercise.Name,
                AuthorId = exercise.AuthorId,
                AuthorName = exercise.User is not null ? exercise.User.Name : "Unknown",
                PictureUrl = pictureService.GetPictureUrl(exercise.PictureUrl),
                Categories = exercise.Categories,
                State = exercise.State,
                IsFavourite = exercise.UsersWhoFavourited.Any(x => x.Id == userCacheService.GetCurrentUserId()),
                ReturnUrl = returnUrl
            };
            bool isAdmin = authorizationService.AuthorizeAsync(User, "AdminOnly").Result.Succeeded;
            ViewBag.IsAdmin = isAdmin;

            return View(model);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Add()
        {
            var categories = await db.Categories.ToListAsync();
            var emptyExercise = new Exercise { Description = "", Name = "" };
            var model = new EditExerciseViewModel { Categories = categories, Exercise = emptyExercise };
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [OutputCache(PolicyName = "NoCache")]
        public async Task<IActionResult> Add(EditExerciseViewModel model)
        {
            var exercise = model.Exercise;
            if (!ModelState.IsValid || exercise == null)
            {
                /*
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
            */
                TempData["Message"] = "Invalid exercise data.";
                model.Categories = await db.Categories.ToListAsync();
                return View(model);
            }
            var isAdmin = (await authorizationService.AuthorizeAsync(User, "AdminOnly")).Succeeded;
            await exerciseService.AddExerciseAsync(isAdmin, exercise, model);
            TempData["Message"] = "Exercise created successfully!";
            return Redirect("/Exercises");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var exercise = await exerciseService.GetExerciseAsync(id, true);
            var isAdmin = (await authorizationService.AuthorizeAsync(User, "AdminOnly")).Succeeded;

            if (exercise == null)
            {
                return NotFound();
            }
            if (!isAdmin && (userCacheService.GetCurrentUserId() != exercise.AuthorId))
            {
                return Forbid();
            }

            var categories = db.Categories.ToList();
            var model = new EditExerciseViewModel { Exercise = exercise, Categories = categories };
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateModelStateFilter]
        [OutputCache(PolicyName = "NoCache")]
        public async Task<IActionResult> Edit(EditExerciseViewModel model)
        {
            var exercise = model.Exercise;
            if (exercise == null)
            {
                return NotFound();
            }
            var isAdmin = (await authorizationService.AuthorizeAsync(User, "AdminOnly")).Succeeded;
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

            exercise.PictureUrl = await fileUploadFacadeService.UploadPicture(model.FileUpload, exercise.PictureUrl, exercise.Id) ?? exercise.PictureUrl;

            db.Exercises.Update(exercise);
            if (db.ChangeTracker.HasChanges())
            {
                await db.SaveChangesAsync();
                await exerciseCacheService.RemoveExerciseFromCacheAsync(exercise.Id);
                TempData["Message"] = "Exercise edited successfully!";
            } else
            {
                TempData["Message"] = "No changes were made.";
            }
            
            

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("profile/{id}/added-exercises")]
        public async Task<IActionResult> UserExercises(int id, int page = 1, int pageSize = 5)
        {
            var isUserExercises = userCacheService.IsCurrentUser(id);
            var username = await userRepository.GetUserNameAsync(id);
            string title;
            IQueryable<Exercise> exercises = db.Exercises.Where(x => x.AuthorId == id).OrderBy(x => x.Id);
            if (isUserExercises)
            {
                title = "My exercises";
            }
            else
            {
                title = $"{username}'s exercises";
            }

            var user = await userRepository.GetUserAsync(id);

            (var items, var model) = await paginationService.GetPaginatedResultAsync(exercises, page, pageSize);

            if (user == null || items == null || items.Count == 0)
            {
                return NotFound();
            }
            if (!isUserExercises && user.IsPublicFavourites)
            {
                return Forbid();
            }

            ViewBag.Title = title;
            ViewBag.Id = id;
            ViewBag.Exercises = items;
            ViewBag.Workout = HttpContext.Session.Get<Workout>("SelectedWorkout");

            return View(model);
        }

        [HttpGet("Profile/{id}/favourite-exercises")]
        [OutputCache()]
        public async Task<IActionResult> Favourites(int id, int page = 1, int pageSize = 5)
        {
            var username = await userRepository.GetUserNameAsync(id);
            var isUserExercises = userCacheService.IsCurrentUser(id);
            IQueryable<Exercise> exercises = db.Users.Where(x => x.Id == id).SelectMany(x => x.FavouriteExercises).OrderBy(x => x.Id);
            var user = await userRepository.GetUserAsync(id);

            (var items, var model) = await paginationService.GetPaginatedResultAsync(exercises, page, pageSize);

            if (user == null || items == null || items.Count == 0)
            {
                return NotFound();
            }
            if (!isUserExercises && user.IsPublicFavourites)
            {
                return Forbid();
            }

            string title;
            if (isUserExercises)
            {
                title = "My favourite exercises";
            }
            else
            {
                title = $"{username}'s favourite exercises";
            }

            ViewBag.Title = title;
            ViewBag.Id = id;
            ViewBag.Exercises = items;
            ViewBag.Workout = HttpContext.Session.Get<Workout>("SelectedWorkout");

            return View("UserExercises", model);
        }

        [OutputCache(PolicyName = "NoCache")]
        public async Task<IActionResult> AddToFavourites(int exerciseId, string? returnUrl)
        {
            Log.VariableLog(logger, nameof(returnUrl), returnUrl ?? "null");
            var exercise = await exerciseService.GetExerciseAsync(exerciseId);
            if (exercise == null)
            {
                return NotFound();
            }
            logger.LogInformation("Exercise is found");
            var user = await userRepository.GetUserAsync(userCacheService.GetCurrentUserId());
            if (user == null)
            {
                return NotFound();
            }
            logger.LogInformation("User is found");
            user.FavouriteExercises.Add(exercise);
            await db.SaveChangesAsync();
            exerciseCacheService.RemoveExerciseFromCacheAsync(exercise.Id);
            userCacheService.RemoveUserFromCacheAsync(user.Id);
            TempData["Message"] = "Exercise added to Favourites successfully!";
            logger.LogInformation("Exercise added to Favourites successfully");

            if (returnUrl is not null && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction(nameof(Details), new { id = exerciseId });
        }

        [OutputCache(PolicyName = "NoCache")]
        public async Task<IActionResult> RemoveFromFavourites(int exerciseId, string? returnUrl)
        {
            var exercise = await exerciseService.GetExerciseAsync(exerciseId, false, ["UsersWhoFavourited"]);
            //var exercise = await db.Exercises.Include(x => x.UsersWhoFavourited).FirstOrDefaultAsync(x => x.Id == exerciseId);
            if (exercise == null)
            {
                return NotFound();
            }
            var user = exercise.UsersWhoFavourited.FirstOrDefault(u => u.Id == userCacheService.GetCurrentUserId());
            if (user == null)
            {
                return NotFound();
            }
            exercise.UsersWhoFavourited.Remove(user);


            await db.SaveChangesAsync();
            exerciseCacheService.RemoveExerciseFromCacheAsync(exercise.Id);
            userCacheService.RemoveUserFromCacheAsync(user.Id);
            TempData["Message"] = "Exercise removed from Favourites successfully :(";
            logger.LogInformation("Exercise removed from Favourites successfully");

            if (returnUrl is not null && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Details), new { id = exerciseId });
        }
    }
}