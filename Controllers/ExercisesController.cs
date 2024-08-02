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
        public ExercisesController(ApplicationContext context, ILogger<ControllerBase> logger, IUserService userService, IFileService fileService, IPictureService pictureService, IPaginationService paginationService)
            : base(context, logger, userService, fileService)
        {
            this.pictureService = pictureService;
            this.paginationService = paginationService;
        }
        public async Task<IActionResult> Index(int page = 1, int pageSize = 5)
        {
            IQueryable<Exercise> exercises = db.Exercises.Where(x => x.State == ExerciseState.Approved).OrderBy(x => x.Id);
            (var items, var model) = await paginationService.GetPaginatedResultAsync(exercises, page, pageSize);
            ViewBag.Exercises = items;
            return View(model);
        }
        public async Task<IActionResult> Details(int id)
        {
            var exercise = await db.Exercises.FirstOrDefaultAsync(x => x.Id == id);
            if (exercise == null)
            {
                return NotFound();
            }
            return View(exercise);
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
                AuthorId = User.Identity!.Name!,
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
