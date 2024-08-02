using Microsoft.AspNetCore.Mvc;
using SportWeb.Models;
using SportWeb.Services;
using SportWeb.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace SportWeb.Controllers
{
    public class ExercisesController : ControllerBase
    {
        IPictureService pictureService;
        IPaginationService paginationService;
        IAuthorizationService authorizationService;
        public ExercisesController(ApplicationContext context, ILogger<ControllerBase> logger, IUserService userService, IFileService fileService, IPictureService pictureService, IPaginationService paginationService, IAuthorizationService authorizationService)
            : base(context, logger, userService, fileService)
        {
            this.pictureService = pictureService;
            this.paginationService = paginationService;
            this.authorizationService = authorizationService;
        }
        public async Task<IActionResult> Index(int page = 1, int pageSize = 5)
        {
            IQueryable<Exercise> exercises = db.Exercises.Where(x => x.State == ExerciseState.Approved).Include(u => u.User).OrderBy(x => x.Id);
            (var items, var model) = await paginationService.GetPaginatedResultAsync(exercises, page, pageSize);
            var result = items.Select(x => new
            {
                x.Id,
                x.Name,
                x.Description,
                x.AuthorId,
                Username = x.User != null ? x.User.Name : "Unknown"
            }).ToList();
            ViewBag.Exercises = result;
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
            return View();
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Add(AddExerciseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var exercise = new Exercise
            {
                Description = model.Description,
                Name = model.Name,
                State = ExerciseState.Pending,
                AuthorId = int.Parse(User.Identity!.Name!)
            };

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
            bool isAdmin = authorizationService.AuthorizeAsync(User, "AdminOnly").Result.Succeeded;

            if (exercise == null)
            {
                return NotFound();
            }
            if (!isAdmin && (User.Identity == null || User.Identity.Name != exercise.AuthorId.ToString())) 
            {
                return Forbid();
            }
            return View();
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(AddExerciseViewModel model)
        {
            return View();
        }
    }
}
