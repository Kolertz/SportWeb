using Microsoft.AspNetCore.Mvc;
using SportWeb.Models;
using SportWeb.Services;
using SportWeb.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using SportWeb.Extensions;

namespace SportWeb.Controllers
{
    public class ExercisesController(ApplicationContext context, ILogger<ControllerBase> logger, IUserService userService, IFileService fileService, IPictureService pictureService, IPaginationService paginationService, IAuthorizationService authorizationService) : ControllerBase(context, logger, userService, fileService)
    {
        public async Task<IActionResult> Index( int? muscle, int? movement, int? tag, int? equipment, string? name, int page = 1, int pageSize = 5)
        {
            IQueryable<Exercise> exercises = db.Exercises.Where(x => x.State == ExerciseState.Approved).Include(u => u.User).Include(c => c.Categories).OrderBy(x => x.Id);
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
            var result = items.Select(x => new
            {
                x.Id,
                x.Name,
                x.Description,
                x.AuthorId,
                Username = x.User != null ? x.User.Name : "Unknown"
            }).ToList();
            ViewBag.Exercises = result;

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
                PaginationModel = paginationModel
            };
            return View(model);
        }
        public async Task<IActionResult> Details(int id)
        {
            var exercise = await db.Exercises.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
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
                State = exercise.State
            };
            bool isAdmin = authorizationService.AuthorizeAsync(User, "AdminOnly").Result.Succeeded;
            ViewBag.IsAdmin = isAdmin;

            return View(model);
        }
        [HttpGet]
        [Authorize]
        public IActionResult Add()
        {
            var categories = db.Categories.ToList();
            var emptyExercise = new Exercise { Description = "", Name = "" };
            var model = new EditExerciseViewModel {  Categories = categories, Exercise = emptyExercise };
            return View(model);
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Add(EditExerciseViewModel model)
        {
            var exercise = model.Exercise;
            if (!ModelState.IsValid || exercise == null)
            {
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
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var exercise = await db.Exercises.FirstOrDefaultAsync(x => x.Id == id);
            var isAdmin = (await authorizationService.AuthorizeAsync(User, "AdminOnly")).Succeeded;

            if (exercise == null)
            {
                return NotFound();
            }
            if (!isAdmin && (User.Identity == null || User.Identity.Name != exercise.AuthorId.ToString())) 
            {
                return Forbid();
            }
            var categories = db.Categories.ToList();
            var model = new EditExerciseViewModel { Exercise = exercise, Categories = categories };
            return View(model);
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(EditExerciseViewModel model)
        {
            var exercise = model.Exercise;
            if (!ModelState.IsValid || exercise == null)
            {
                logger.LogWarning("Something went wrong!");
                return View(model);
            }
            var isAdmin = (await authorizationService.AuthorizeAsync(User, "AdminOnly")).Succeeded;
            if (!isAdmin)
            {
                exercise.State = ExerciseState.Pending;
                logger.LogInformation("Exercise state changed to pending");
            } else
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
        [Route("Profile/{id}/Exercises")]
        public async Task<IActionResult> UserExercises(int id, int page = 1, int pageSize = 5, string username = "???")
        {
            var isUserExercises = userService.IsCurrentUser(id);
            IQueryable<Exercise> exercises = db.Exercises.OrderBy(x => x.Id);
            (var items, var model) = await paginationService.GetPaginatedResultAsync(exercises, page, pageSize);
            ViewBag.Id = id;
            ViewBag.Exercises = items;
            ViewBag.Username = username;
            ViewBag.IsUserExercises = isUserExercises;
            ViewBag.Workout = HttpContext.Session.Get<Workout>("SelectedWorkout");
            return View(model);
        }
    }
}
