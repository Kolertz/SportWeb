using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using SportWeb.Models.Entities;
using SportWeb.Services;

namespace SportWeb.Tests
{
    public class AvatarServiceTests
    {
        private readonly AvatarService avatarService;
        private readonly Mock<IWebHostEnvironment> mockEnv;
        private readonly Mock<ILogger<AvatarService>> mockLogger;

        public AvatarServiceTests()
        {
            mockLogger = new();
            mockEnv = new();
            avatarService = new
                (
                mockLogger.Object,
                mockEnv.Object
                );
        }

        [Fact]
        public void NewAvatarName_DefaultAvatar_ReturnsNewAvatarName()
        {
            // Arrange
            var user = new User { Id = 1, Avatar = "avatar.png", Email = "", Password = "" };

            // Act
            var result = avatarService.NewAvatarName(user.Avatar, user.Id);

            // Assert
            Assert.Equal("avatar1_v=1.png", result);
        }

        [Fact]
        public void NewAvatarName_AvatarWithVersion_ReturnsIncrementedVersion()
        {
            // Arrange
            var user = new User { Id = 1, Avatar = "avatar1_v=2.png", Email = "", Password = "" };

            // Act
            var result = avatarService.NewAvatarName(user.Avatar, user.Id);

            // Assert
            Assert.Equal("avatar1_v3.png", result);
        }

        [Fact]
        public void NewAvatarName_NullUser_ThrowsArgumentException()
        {
            // Arrange
            User? user = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => avatarService.NewAvatarName(user!.Avatar, user.Id));
            Assert.Equal("User or user avatar cannot be null", exception.Message);
        }

        [Fact]
        public void NewAvatarName_UserWithNullAvatar_ThrowsArgumentException()
        {
            // Arrange
            var user = new User { Id = 1, Avatar = "", Email = "", Password = "" };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => avatarService.NewAvatarName(user.Avatar, user.Id));
            Assert.Equal("User or user avatar cannot be null", exception.Message);
        }
    }
}