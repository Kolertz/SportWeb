namespace SportWeb.Services
{
    public interface IFileService
    {
        Task<bool> UploadFileToServer(IFormFile fileUpload, string filePath);
    }

    public class FileService(ILogger<FileService> logger) : IFileService
    {
        public async Task<bool> UploadFileToServer(IFormFile fileUpload, string filePath)
        {
            try
            {
                if (fileUpload == null || fileUpload.Length == 0)
                {
                    logger.LogWarning("File upload failed: file is null or empty.");
                    return false;  // Прекратить выполнение метода, если файл не подходит для загрузки.
                }

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await fileUpload.CopyToAsync(stream);
                }

                logger.LogInformation($"File {fileUpload.FileName} uploaded to {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error {ex} when trying to upload file");
                return false;
            }
        }
    }
}