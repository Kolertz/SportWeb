using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SportWeb.Controllers
{
    public class WorkoutController : Controller
    {
        // GET: WorkoutController
        public ActionResult Index()
        {
            return View();
        }

        // GET: WorkoutController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: WorkoutController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: WorkoutController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: WorkoutController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: WorkoutController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: WorkoutController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: WorkoutController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
