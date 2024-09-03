using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportWeb.Models;
using SportWeb.Services;
using System.Diagnostics;

namespace SportWeb.Controllers
{
    public class HomeController(IUserRepository userRepository, IAuthorizationService authorizationService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            bool isAdmin = authorizationService.AuthorizeAsync(User, "AdminOnly").Result.Succeeded;
            ViewBag.IsAdmin = isAdmin;
            var user = await userRepository.GetUserAsync(User.Identity!.Name, true);
            ViewBag.User = user;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var errorViewModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };
            return View(errorViewModel); // Передаем модель в представление
        }
    }
}