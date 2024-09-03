using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SportWeb.Extensions;
using SportWeb.Filters;
using SportWeb.Models;
using SportWeb.Models.Entities;
using SportWeb.Services;

namespace SportWeb.Controllers
{
    public class ExercisesController(
        ApplicationContext db,
        IExerciseService exerciseService,
        ILogger<ExercisesController> logger,
        IUserRepository userRepository,
        IUserSessionService userSessionService,
        IFileService fileService,
        IPictureService pictureService,
        IPaginationService paginationService,
        IAuthorizationService authorizationService) : Controller
    {
        public async Task<IActionResult> Index(int? muscle, int? movement, int? tag, int? equipment, string? name, int page = 1, int pageSize = 5)
        {
            IQueryable<Exercise> exercises = db.Exercises.Where(x => x.State == ExerciseState.Approved).Include(u => u.User).Include(c => c.Categories).Include(u => u.UsersWhoFavourited).OrderBy(x => x.Name);

            if (muscle != null && muscle != 0)
            {
                exercises = exercises.Where(e => e.Categories!.Any(c => c.Id == muscle));
            }
            if (movement != null && movement != 0)
            {
                exercises = exercises.Where(e => e.Categories!.Any(c => c.Id == movement));
            }
            if (tag != null && tag != 0)
            {
                exercises = exercises.Where(e => e.Categories!.Any(c => c.Id == tag));
            }
            if (equipment != null && equipment != 0)
            {
                exercises = exercises.Where(e => e.Categories!.Any(c => c.Id == equipment));
            }
            if (name != null && name.Length > 1)
            {
                exercises = exercises.Where(p => p.Name.Contains(name));
            }
            (var items, var paginationModel) = await paginationService.GetPaginatedResultAsync(exercises, page, pageSize);
            var result = items.Select(x => new IndexExerciseViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                AuthorId = x.AuthorId,
                Username = x.User != null ? x.User.Name : "Unknown",
                IsFavourite = x.UsersWhoFavourited.Any(u => u.Id == userSessionService.GetCurrentUserId())
            }).ToList();

            var categories = db.Categories.ToList();
            var movements = categories.Where(x => x.Type == "Movement Pattern").ToList();
            var tags = categories.Where(x => x.Type == "Other").ToList();
            var equipments = categories.Where(x => x.Type == "Equipment").ToList();
            var muscles = categories.Where(x => x.Type == "Muscle Group").ToList();

            movements.Insert(0, new Category { Name = "All", Id = 0 });
            tags.Insert(0, new Category { Name = "All", Id = 0 });
            equipments.Insert(0, new Category { Name = "All", Id = 0 });
            muscles.Insert(0, new Category { Name = "All", Id = 0 });

            ViewBag.SelectedWorkout = HttpContext.Session.Get<Workout>("SelectedWorkout");

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
                Exercises = result
            };
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var exercise = await db.Exercises.Include(x => x.User).Include(x => x.Categories).Include(x => x.UsersWhoFavourited).FirstOrDefaultAsync(x => x.Id == id);
            if (exercise == null)
            {
                return NotFound();
            }
            var model = new ExerciseDetailsViewModel
            {
                Id = exercise.Id,
                Description = exercise.Description,
                Name = exercise.Name,
                AuthorId = exercise.AuthorId,
                AuthorName = exercise.User != null ? exercise.User.Name : "Unknown",
                PictureUrl = pictureService.GetPictureUrl(exercise.PictureUrl),
                Categories = exercise.Categories,
                State = exercise.State,
                IsFavourite = exercise.UsersWhoFavourited.Any(x => x.Id == userSessionService.GetCurrentUserId()),
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
            exercise.AuthorId = int.Parse(User.Identity!.Name!);
            exercise.Categories = (List<Category>?)model.SelectedCategories;

            var fileUpload = model.FileUpload;
            if (fileUpload != null && fileUpload.Length > 0)
            {
                var filePath = pictureService.GetPicturePath(pictureService.NewPictureName(exercise));
                await fileService.UploadFile(fileUpload, filePath);
            }

            await db.Exercises.AddAsync(exercise);
            await db.SaveChangesAsync();
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
            if (!isAdmin && (userSessionService.GetCurrentUserId() != exercise.AuthorId))
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
        public async Task<IActionResult> Edit(EditExerciseViewModel model)
        {
            var exercise = model.Exercise;
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

            var fileUpload = model.FileUpload;
            if (fileUpload != null && fileUpload.Length > 0)
            {
                exercise.PictureUrl = pictureService.NewPictureName(exercise);
                var filePath = pictureService.GetPicturePath(exercise.PictureUrl);
                await fileService.UploadFile(fileUpload, filePath);
            }

            db.Exercises.Update(exercise);
            await db.SaveChangesAsync();
            TempData["Message"] = "Exercise edited successfully!";

            return RedirectToAction(nameof(Index));
        }

        [Route("profile/{id}/added-exercises")]
        public async Task<IActionResult> UserExercises(int id, int page = 1, int pageSize = 5, string username = "???")
        {
            var isUserExercises = userSessionService.IsCurrentUser(id);

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

        [HttpGet]
        [Route("Profile/{id}/favourite-exercises")]
        public async Task<IActionResult> Favourites(int id, int page = 1, int pageSize = 5, string username = "???")
        {
            var isUserExercises = userSessionService.IsCurrentUser(id);
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

        public async Task<IActionResult> AddToFavourites(int exerciseId, string? returnUrl)
        {
            Log.VariableLog(logger, nameof(returnUrl), returnUrl ?? "null");
            var exercise = await exerciseService.GetExerciseAsync(exerciseId);
            if (exercise == null)
            {
                return NotFound();
            }
            logger.LogInformation("Exercise is found");
            var user = await userRepository.GetUserAsync(userSessionService.GetCurrentUserId());
            if (user == null)
            {
                return NotFound();
            }
            logger.LogInformation("User is found");
            user.FavouriteExercises.Add(exercise);
            await db.SaveChangesAsync();

            TempData["Message"] = "Exercise added to Favourites successfully!";
            logger.LogInformation("Exercise added to Favourites successfully");

            if (returnUrl != null && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction(nameof(Details), new { id = exerciseId });
        }

        public async Task<IActionResult> RemoveFromFavourites(int exerciseId, string? returnUrl)
        {
            //var exercise = await exerciseService.GetExerciseAsync(exerciseId, false, ["UsersWhoFavourited"]);
            var exercise = await db.Exercises.Include(x => x.UsersWhoFavourited).FirstOrDefaultAsync(x => x.Id == exerciseId);
            if (exercise == null)
            {
                return NotFound();
            }
            var user = exercise.UsersWhoFavourited.FirstOrDefault(u => u.Id == userSessionService.GetCurrentUserId());
            if (user == null)
            {
                return NotFound();
            }
            exercise.UsersWhoFavourited.Remove(user);

            if (db.ChangeTracker.HasChanges())
            {
                await db.SaveChangesAsync();
                TempData["Message"] = "Exercise removed from Favourites successfully :(";
                logger.LogInformation("Exercise removed from Favourites successfully");
            }
            else
            {
                logger.LogError($"Change tracker has no changes, User id is {user.Id}, exercise id is {exercise.Id}");
            }

            if (returnUrl != null && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Details), new { id = exerciseId });
        }
    }
}