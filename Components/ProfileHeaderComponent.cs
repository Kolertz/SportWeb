using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using SportWeb.Extensions;
using SportWeb.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using SportWeb.Models.Entities;
namespace SportWeb.Components
{
    public class ProfileHeaderComponent : ViewComponent
    {
        readonly IUserService userService;
        readonly ILogger logger;
        readonly IAvatarService avatarService;
        public ProfileHeaderComponent(IUserService userService, ILogger<ProfileHeaderComponent> logger, IAvatarService avatarService)
        {
            this.userService = userService;
            this.logger = logger;
            this.avatarService = avatarService;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                string id = User.Identity.Name!;
                logger.LogInformation($"User is authenticated, his id is {id}");
                User? user = await userService.GetUserAsync(id);
                if (user != null)
                {
                    logger.LogInformation($"User name: {user.Name}");
                    var avatar = avatarService.GetAvatarUrl(user.Avatar);
                    var modelAuth = new
                    {
                        UserName = user.Name,
                        Avatar = avatar,
                        Id = id,
                    };
                    ViewBag.Model = modelAuth;
                    logger.LogInformation($"Avatar url: {avatar}");
                    return View("ProfileHeader");
                }
            }
            var model = new
            {
                UserName = "Anonymous",
                Avatar = "img/avatar.png",
                Id = "",
            };
            ViewBag.Model = model;
            return View("ProfileHeader");
        }
    }
}
