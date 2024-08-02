using Microsoft.Extensions.Logging;
using SportWeb.Models.Entities;

namespace SportWeb.Services
{
    public interface IFileService
    {
        Task UploadFile(IFormFile fileUpload, string filePath);
    }
    public class FileService : IFileService
    {
        private readonly ILogger<FileService> logger;

        public FileService(ILogger<FileService> logger)
        {
            this.logger = logger;
        }

        public async Task UploadFile(IFormFile fileUpload, string filePath)
        {
            try
            {
                if (fileUpload != null && fileUpload.Length > 0)
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await fileUpload.CopyToAsync(stream);
                    }
                    logger.LogInformation($"File {fileUpload.FileName} uploaded to {filePath}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Error {ex} when trying to upload file");
            }
        }
    }
}
