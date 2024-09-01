using Microsoft.AspNetCore.Mvc;
using SportWeb.Models;
using SportWeb.Models.Entities;
using SportWeb.Services;
namespace SportWeb.Components
{
    public class ProfileHeaderComponent(IUserSessionService userSessionService, IUserRepository userRepository, ILogger<ProfileHeaderComponent> logger, IAvatarService avatarService) : ViewComponent
    {
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
                string? id = userSessionService.GetCurrentUserId().ToString();
                User? user = await userRepository.GetUserAsync(id, true);
                if (user != null)
                {
                    logger.LogInformation($"User is authenticated, his name: {user.Name}");
                    var avatar = avatarService.GetAvatarUrl(user.Avatar);
                    model.UserName = user.Name ?? "Anonymous";
                    model.Avatar = avatar;
                    model.Id = id;
                }
            } else
            {
                logger.LogInformation("User is not authenticated");
            }
            
            return View("ProfileHeader", model);
        }
    }
}
