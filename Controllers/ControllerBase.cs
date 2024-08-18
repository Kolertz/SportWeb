using SportWeb.Models;
using Microsoft.AspNetCore.Mvc;
using SportWeb.Services;
namespace SportWeb.Controllers
{
    public abstract class ControllerBase(ApplicationContext context, ILogger<ControllerBase> logger, IUserService userService, IFileService fileService) : Controller
    {
        public readonly ApplicationContext db = context;
        public readonly ILogger<ControllerBase> logger = logger;
        public readonly IUserService userService = userService;
        public readonly IFileService fileService = fileService;
    }
}
