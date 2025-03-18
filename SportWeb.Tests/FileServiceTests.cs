using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using SportWeb.Services;

namespace SportWeb.Tests
{
    public class FileServiceTests
    {
        private readonly Mock<ILogger<FileService>> _loggerMock;
        private readonly FileService _fileService;

        public FileServiceTests()
        {
            _loggerMock = new Mock<ILogger<FileService>>();
            _fileService = new FileService(_loggerMock.Object);
        }

        [Fact]
        public async Task UploadFile_ValidFile_FileUploaded()
        {
            // Arrange
            var fileUpload = new Mock<IFormFile>();
            fileUpload.Setup(f => f.Length).Returns(100);
            fileUpload.Setup(f => f.FileName).Returns("test.txt");
            var filePath = "test.txt";

            // Act
            await _fileService.UploadFileToServer(fileUpload.Object, filePath);

            // Assert
            Assert.True(File.Exists(filePath));
        }

        [Fact]
        public async Task UploadFile_NullFile_FileNotUploaded()
        {
            // Arrange
            IFormFile fileUpload = null!;
            var filePath = "test.txt";
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            // Act
            await _fileService.UploadFileToServer(fileUpload!, filePath);

            // Assert
            Assert.False(File.Exists(filePath));
        }

        [Fact]
        public async Task UploadFile_EmptyFile_FileNotUploaded()
        {
            // Arrange
            var fileUpload = new Mock<IFormFile>();
            fileUpload.Setup(f => f.Length).Returns(0);
            var filePath = "test.txt";
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            // Act
            await _fileService.UploadFileToServer(fileUpload.Object, filePath);

            // Assert
            Assert.False(File.Exists(filePath));
        }
    }
}