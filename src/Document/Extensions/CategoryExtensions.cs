using Domain.Entities;
using Domain.Filters;
using Microsoft.EntityFrameworkCore;

namespace Extensions;

public static class CategoryExtensions
{
    public static IQueryable<Category> ApplyFilters(this IQueryable<Category> query, CategoryFilterParams filterParams)
    {
        if (!string.IsNullOrEmpty(filterParams.Keyword))
        {
            var lowerKeyword = filterParams.Keyword.ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(lowerKeyword));
        }

        if (!string.IsNullOrWhiteSpace(filterParams.Name))
        {
            var lowerName = filterParams.Name.ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(lowerName));
        }

        if (filterParams.CreatedAfter.HasValue)
        {
            query = query.Where(c => c.CreatedAt >= filterParams.CreatedAfter.Value);
        }

        if (filterParams.CreatedBefore.HasValue)
        {
            query = query.Where(c => c.CreatedAt <= filterParams.CreatedBefore.Value);
        }

        return query;
    }

    public static IQueryable<Category> ApplySort(this IQueryable<Category> query, string sortBy, bool isDescending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            sortBy = "CreatedAt";
        }

        query = sortBy.ToLower() switch
        {
            "name" => isDescending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
            "createdat" => isDescending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
            "updatedat" => isDescending ? query.OrderByDescending(c => c.UpdatedAt) : query.OrderBy(c => c.UpdatedAt),
            _ => isDescending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt)
        };

        return query;
    }
}
