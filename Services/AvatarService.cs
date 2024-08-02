using Microsoft.Extensions.Logging;
using SportWeb.Models.Entities;
using System.Text.RegularExpressions;

namespace SportWeb.Services
{
    public interface IAvatarService
    {
        string NewAvatarName(User user);
        string GetAvatarPath(string avatarName);
        string GetAvatarUrl(string avatarName);
    }
    public class AvatarService : IAvatarService
    {
        private readonly ILogger logger;
        private readonly IWebHostEnvironment env;
        public AvatarService(ILogger<AvatarService> logger, IWebHostEnvironment env)
        {
            this.logger = logger;
            this.env = env;
        }
        public string NewAvatarName(User user)
        {

            if (user == null || string.IsNullOrEmpty(user.Avatar))
            {
                throw new ArgumentException("User or user avatar cannot be null");
            }

            string avatarUrl = user.Avatar;
            if (avatarUrl == "avatar.png")
            {
                return $"avatar{user.Id}_v=1.png";
            }
            //string pattern = @"\_v=(\d+)\.png";
            string pattern = @"_v=(\d+)\.png";
            var match = Regex.Match(avatarUrl, pattern);

            if (match.Success && int.TryParse(match.Groups[1].Value, out int version))
            {
                version++;
                string path = $"avatar{user.Id}_v{version}.png";
                logger.LogInformation($"New avatar path: {path}");
                return path;
            }

            throw new FormatException("The avatar URL format is incorrect.");
        }
        public string GetAvatarUrl(string avatarName)
        {
            string fileUrl = $"/img/{avatarName}";

            return fileUrl;
        }
        public string GetAvatarPath(string avatarName)
        {
            // Получаем путь до wwwroot
            string webRootPath = env.WebRootPath;
            // Строим полный путь до файла
            string avatarPath = Path.Combine(webRootPath, "img", avatarName);
            if (!File.Exists(avatarPath))
            {
                 avatarPath = Path.Combine(webRootPath, "img", "avatar.png");
            }
            return avatarPath;
        }
    }
}
