using SportWeb.Models;
using Microsoft.AspNetCore.Mvc;
using SportWeb.Services;
namespace SportWeb.Controllers
{
    public abstract class ControllerBase : Controller
    {
        public readonly ApplicationContext db;
        public readonly ILogger<ControllerBase> logger;
        public readonly IUserService userService;
        public readonly IFileService fileService;
        public ControllerBase(ApplicationContext context, ILogger<ControllerBase> logger, IUserService userService, IFileService fileService)
        {
            db = context;
            this.logger = logger;
            this.userService = userService;
            this.fileService = fileService;
        }
    }
}
