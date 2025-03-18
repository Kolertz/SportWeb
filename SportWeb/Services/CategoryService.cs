using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SportWeb.Models;
using SportWeb.Models.Entities;

namespace SportWeb.Services
{
    public interface ICategoryService
    {
        Task<(List<Category> movements, List<Category> tags, List<Category> equipments, List<Category> muscles)> GetCategoryFiltersAsync();
    }
    public class CategoryService(ApplicationContext db, IMemoryCache memoryCache, ILogger<CategoryService> logger) : ICategoryService
    {
        public async Task<(List<Category> movements, List<Category> tags, List<Category> equipments, List<Category> muscles)> GetCategoryFiltersAsync()
        {
            var categories = await db.Categories.ToListAsync();
            SetCategoriesToCache(categories);

            var movements = categories.Where(x => x.Type == "Movement Pattern").ToList();
            var tags = categories.Where(x => x.Type == "Other").ToList();
            var equipments = categories.Where(x => x.Type == "Equipment").ToList();
            var muscles = categories.Where(x => x.Type == "Muscle Group").ToList();

            movements.Insert(0, new Category { Name = "All", Id = 0 });
            tags.Insert(0, new Category { Name = "All", Id = 0 });
            equipments.Insert(0, new Category { Name = "All", Id = 0 });
            muscles.Insert(0, new Category { Name = "All", Id = 0 });

            return (movements, tags, equipments, muscles);
        }

        public void SetCategoriesToCache(List<Category> categories)
        {
            memoryCache.Set("categories", categories, new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromHours(6) // Настройка времени жизни в кэше
            });
            logger.LogInformation("Categories were set to cache");
        }

        public void RemoveCategoriesFromCache()
        {
            memoryCache.Remove("categories");
        }
    }
}
