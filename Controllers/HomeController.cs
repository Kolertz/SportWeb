using Microsoft.AspNetCore.Mvc;
using SportWeb.Models;
using System.Diagnostics;
using SportWeb.Services;
using Microsoft.AspNetCore.Authorization;

namespace SportWeb.Controllers
{
    public class HomeController : ControllerBase
    {
        IAuthorizationService authorizationService;
        public HomeController(ApplicationContext context, ILogger<AccountController> logger, IUserService userService, IAuthorizationService authorizationService, IFileService fileService)
            : base(context, logger, userService, fileService)
        {
            this.authorizationService = authorizationService;
        }

        public IActionResult Index()
        {
            bool isAdmin = authorizationService.AuthorizeAsync(User, "AdminOnly").Result.Succeeded;
            ViewBag.IsAdmin = isAdmin;
            return View();
        }
       
    }
}
