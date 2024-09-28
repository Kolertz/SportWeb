using SportWeb.Models.Entities;

namespace SportWeb.Services
{
    public interface IFileUploadFacadeService
    {
        Task<string?> UploadFile(IFormFile? fileUpload, string userAvatar, int userId);
        Task<string?> UploadPicture(IFormFile? fileUpload, string pictureUrl, int exerciseId);
    }
    public class FileUploadFacadeService(ILogger<FileUploadFacadeService> logger, IFileService fileService, IAvatarService avatarService, IPictureService pictureService) : IFileUploadFacadeService
    {
        public async Task<string?> UploadFile(IFormFile? fileUpload, string userAvatar, int userId)
        {
            if (fileUpload is not null && fileUpload.Length > 0)
            {
                var oldUserAvatar = userAvatar;

                userAvatar = avatarService.NewAvatarName(userAvatar, userId);
                var filePath = avatarService.GetAvatarPath(userAvatar);
                var IsFileUploaded = await fileService.UploadFileToServer(fileUpload, filePath);

                if (!IsFileUploaded)
                {
                    return oldUserAvatar;
                }

                return userAvatar;
            }
            return null;
        }
        public async Task<string?> UploadPicture(IFormFile? fileUpload, string pictureUrl, int exerciseId)
        {
            if (fileUpload is not null && fileUpload.Length > 0)
            {
                var oldPictureUrl = pictureUrl;

                pictureUrl = pictureService.NewPictureName(pictureUrl, exerciseId);
                var filePath = pictureService.GetPicturePath(pictureUrl);
                var IsFileUploaded = await fileService.UploadFileToServer(fileUpload, filePath);

                if (!IsFileUploaded)
                {
                    return oldPictureUrl;
                }

                return pictureUrl;
            }
            return null;
        }
    }
}
