using Microsoft.Extensions.Logging;
using SportWeb.Models.Entities;

namespace SportWeb.Services
{
    public interface IFileService
    {
        Task UploadFile(IFormFile fileUpload, string filePath);
    }
    public class FileService(ILogger<FileService> logger) : IFileService
    {
        public async Task UploadFile(IFormFile fileUpload, string filePath)
        {
            try
            {
                if (fileUpload == null || fileUpload.Length == 0)
                {
                    logger.LogWarning("File upload failed: file is null or empty.");
                    return;  // Прекратить выполнение метода, если файл не подходит для загрузки.
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await fileUpload.CopyToAsync(stream);
                }

                logger.LogInformation($"File {fileUpload.FileName} uploaded to {filePath}");
            }
            catch (Exception ex)
            {
                logger.LogError($"Error {ex} when trying to upload file");
            }
        }

    }
}
