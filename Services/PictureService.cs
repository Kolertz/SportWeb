using Microsoft.Extensions.Logging;
using SportWeb.Models.Entities;
using System.Text.RegularExpressions;

namespace SportWeb.Services
{
    public interface IPictureService
    {
        string NewPictureName(Exercise exercise);
        string GetPicturePath(string pictureName);
        string GetPictureUrl(string pictureName);
    }
    public class PictureService(ILogger<PictureService> logger, IWebHostEnvironment env) : IPictureService
    {
        private readonly ILogger logger = logger;

        public string NewPictureName(Exercise exercise)
        {
            string pictureUrl = exercise.PictureUrl;
            if (pictureUrl == "picture.png")
            {
                return $"picture{exercise.Id}_v=1.png";
            }
            string pattern = @"_v=(\d+)\.png";
            var match = Regex.Match(pictureUrl, pattern);

            if (match.Success && int.TryParse(match.Groups[1].Value, out int version))
            {
                version++;
                string path = $"{exercise.Id}_v{version}.png";
                logger.LogInformation($"New picture path: {path}");
                return path;
            }

            throw new FormatException("The picture URL format is incorrect.");
        }
        public string GetPictureUrl(string pictureName)
        {
            string fileUrl = $"/img/exercises/{pictureName}";

            return fileUrl;
        }
        public string GetPicturePath(string pictureName)
        {
            // Получаем путь до wwwroot
            string webRootPath = env.WebRootPath;
            // Строим полный путь до файла
            string picturePath = Path.Combine(webRootPath, "img","exercises", pictureName);
            if (!File.Exists(picturePath))
            {
                picturePath = Path.Combine(webRootPath, "img", "exercises", "picture.png");
            }
            return picturePath;
        }
    }
}