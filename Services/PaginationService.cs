using Microsoft.EntityFrameworkCore;
using SportWeb.Models.Entities;
using SportWeb.Models;

namespace SportWeb.Services
{
    public interface IPaginationService
    {
        Task<(List<T> items, PaginationModel)> GetPaginatedResultAsync<T>(IQueryable<T> query, int page, int pageSize);
    }
    public class PaginationService : IPaginationService
    {
        public PaginationService() { }
        public async Task<(List<T> items, PaginationModel)> GetPaginatedResultAsync<T>(IQueryable<T> query, int page, int pageSize)
        {
            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return (
                items,
                new PaginationModel
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalItems = total,
                });
        }
    }
}
