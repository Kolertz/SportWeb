using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using SportWeb.Extensions;
using SportWeb.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using SportWeb.Models.Entities;
using SportWeb.Models;
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
            var model = new ProfileHeaderViewModel
            {
                UserName = "Anonymous",
                Avatar = "img/avatar.png",
                Id = "",
            };
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                string id = User.Identity.Name!;
                logger.LogWarning($"User is authenticated, his id is {id}");
                User? user = await userService.GetUserAsync(id, true);
                if (user != null)
                {
                    logger.LogInformation($"User name: {user.Name}");
                    var avatar = avatarService.GetAvatarUrl(user.Avatar);
                    model.UserName = user.Name;
                    model.Avatar = avatar;
                    model.Id = id;
                }
            } else
            {
                logger.LogWarning("User is not authenticated");
            }
            
            return View("ProfileHeader", model);
        }
    }
}
