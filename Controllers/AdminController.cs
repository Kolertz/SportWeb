using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportWeb.Models;
using SportWeb.Models.Entities;
using SportWeb.Services;

namespace SportWeb.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : ControllerBase
    {
        IPaginationService paginationService;
        public AdminController(ApplicationContext context, ILogger<ControllerBase> logger, IUserService userService, IFileService fileService, IPaginationService paginationService)
            :base(context, logger, userService, fileService)
        {
            this.paginationService = paginationService;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> PendingExercises(int page = 1, int pageSize = 5)
        {
            IQueryable<Exercise> exercises = db.Exercises.Where(x => x.State == ExerciseState.Pending).OrderBy(x => x.Id);
            (var items, var model) = await paginationService.GetPaginatedResultAsync(exercises, page, pageSize);
            ViewBag.Exercises = items;
            return View(model);
        }
        public async Task<IActionResult> RejectedExercises(int page = 1, int pageSize = 5)
        {
            IQueryable<Exercise> exercises = db.Exercises.Where(x => x.State == ExerciseState.Rejected).OrderBy(x => x.Id);
            (var items, var model) = await paginationService.GetPaginatedResultAsync(exercises, page, pageSize);
            ViewBag.Exercises = items;
            return View(model);
        }
        public async Task<IActionResult> Approve(int id)
        {
            var exercise = db.Exercises.FirstOrDefault(x => x.Id == id);
            if (exercise == null) 
            { 
                TempData["Message"] = "Exercise not found!";
                logger.LogWarning("Exercise not found!");
                return RedirectToAction(nameof(PendingExercises));
            }
            exercise.State = ExerciseState.Approved;
            await db.SaveChangesAsync();
            TempData["Message"] = "Exercise approved successfully!";
            logger.LogInformation("Exercise approved successfully!");
            return RedirectToAction(nameof(PendingExercises));
        }
    }
}
