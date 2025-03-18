using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using SportWeb.Models;
using SportWeb.Models.Entities;
using SportWeb.Services;

namespace SportWeb.Tests
{
    public class UserServiceTests
    {
        private readonly UserService userService;
        private readonly Mock<ILogger<UserService>> mockLogger;
        private readonly ApplicationContext db;
        private readonly Mock<IPasswordCryptorService> mockPasswordCryptor;
        private readonly Mock<IHttpContextAccessor> mockHttpContextAccessor;
        private readonly Mock<IMemoryCache> mockMemoryCache;
        private readonly Mock<IOutputCacheStore> mockOutputCacheStore;

        public UserServiceTests()
        {
            mockLogger = new Mock<ILogger<UserService>>();
            mockPasswordCryptor = new Mock<IPasswordCryptorService>();
            mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockMemoryCache = new Mock<IMemoryCache>();
            mockOutputCacheStore = new Mock<IOutputCacheStore>();

            var options = new DbContextOptionsBuilder<ApplicationContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;
            var env = new Mock<IWebHostEnvironment>();
            db = new ApplicationContext(options, env.Object);

            userService = new UserService(
                db,
                mockLogger.Object,
                mockPasswordCryptor.Object,
                mockHttpContextAccessor.Object,
                mockMemoryCache.Object,
                mockOutputCacheStore.Object
                );
        }

        [Fact]
        public async Task GetUserAsync_ExistingUserId_ReturnsUserFromDatabase()
        {
            // Arrange
            int userId = 2;
            var user = new User { Id = userId, Email = "Test", Password = "Test" };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            // Act
            var result = await userService.GetUserAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
        }

        [Fact]
        public async Task GetUserAsync_NonExistingUserId_ReturnsNull()
        {
            // Arrange
            int userId = 3;

            // Act
            var result = await userService.GetUserAsync(userId);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData(10, 10)]
        [InlineData("20", 20)]
        [InlineData("abc", 0)]
        [InlineData(42.0, 0)]
        public void CastToInt_WithVariousInputs_ReturnsExpectedResult(object input, int expected)
        {
            // Act
            int result = userService.CastToInt(input);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}