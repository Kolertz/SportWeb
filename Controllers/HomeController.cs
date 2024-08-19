using Microsoft.AspNetCore.Mvc;
using SportWeb.Models;
using System.Diagnostics;
using SportWeb.Services;
using Microsoft.AspNetCore.Authorization;
using SportWeb.Models.Entities;
using SportWeb.Extensions;

namespace SportWeb.Controllers
{
    public class HomeController(IUserService userService, IAuthorizationService authorizationService) : Controller
    {
        public async Task <IActionResult> Index()
        {
            bool isAdmin = authorizationService.AuthorizeAsync(User, "AdminOnly").Result.Succeeded;
            ViewBag.IsAdmin = isAdmin;
            var user = await userService.GetUserAsync(User.Identity!.Name, true);
            ViewBag.User = user;
            return View();
        }
       
    }
}
