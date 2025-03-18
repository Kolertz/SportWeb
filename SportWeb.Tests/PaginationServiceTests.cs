using Microsoft.EntityFrameworkCore;
using Moq;
using SportWeb.Models.Entities;
using SportWeb.Services;

namespace SportWeb.Tests
{
    public class PaginationServiceTests
    {
        private readonly Mock<DbSet<Exercise>> _mockExercisesDbSet;
        private readonly Mock<DbContext> _mockDbContext;
        private readonly PaginationService _paginationService;

        public PaginationServiceTests()
        {
            _mockExercisesDbSet = new Mock<DbSet<Exercise>>();
            _mockDbContext = new Mock<DbContext>();
            _paginationService = new PaginationService();
        }

        [Fact]
        public async Task GetPaginatedResultAsync_ReturnsExpectedResults()
        {
            // Arrange
            var exercises = new List<Exercise>
            {
                new() { Id = 1, Name = "Exercise 1", Description = "Description" },
                new() { Id = 2, Name = "Exercise 2", Description = "Description" },
                new() { Id = 3, Name = "Exercise 3", Description = "Description" },
                new() { Id = 4, Name = "Exercise 4", Description = "Description" },
                new() { Id = 5, Name = "Exercise 5", Description = "Description" },
            }.AsQueryable();

            _mockExercisesDbSet.As<IQueryable<Exercise>>().Setup(m => m.Provider).Returns(exercises.Provider);
            _mockExercisesDbSet.As<IQueryable<Exercise>>().Setup(m => m.Expression).Returns(exercises.Expression);
            _mockExercisesDbSet.As<IQueryable<Exercise>>().Setup(m => m.ElementType).Returns(exercises.ElementType);
            _mockExercisesDbSet.As<IQueryable<Exercise>>().Setup(m => m.GetEnumerator()).Returns(exercises.GetEnumerator());

            // Mock ToListAsync
            _mockExercisesDbSet.Setup(m => m.ToListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(exercises.ToList());

            _mockDbContext.Setup(c => c.Set<Exercise>()).Returns(_mockExercisesDbSet.Object);

            // Act
            var (result, model) = await _paginationService.GetPaginatedResultAsync(_mockDbContext.Object.Set<Exercise>(), 2, 2);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(3, result[0].Id);
            Assert.Equal(4, result[1].Id);
            Assert.Equal(2, model.CurrentPage);
            Assert.Equal(2, model.PageSize);
            Assert.Equal(5, model.TotalItems);
        }
    }
}