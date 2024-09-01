using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace SportWeb.Extensions
{
    public static class QueryableExtension
    {
        public static IQueryable<T> IncludeProperty<T>(this IQueryable<T> query, Expression<Func<T, object>> navigationPropertyPath) where T : class
        {
            return query.Include(navigationPropertyPath);
        }

    }
}
