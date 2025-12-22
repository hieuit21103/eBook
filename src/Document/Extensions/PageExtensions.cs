using Domain.Entities;
using Domain.Filters;
using Microsoft.EntityFrameworkCore;

namespace Extensions;

public static class PageExtensions
{
    public static IQueryable<Page> ApplyFilters(this IQueryable<Page> query, PageFilterParams filterParams)
    {
        query = query.Include(p => p.Document);

        if (filterParams.DocumentId.HasValue)
        {
            query = query.Where(p => p.DocumentId == filterParams.DocumentId.Value);
        }

        if (filterParams.SpecificPageNumber.HasValue)
        {
            query = query.Where(p => p.PageNumber == filterParams.SpecificPageNumber.Value);
        }

        if (filterParams.CreatedAfter.HasValue)
        {
            query = query.Where(p => p.CreatedAt >= filterParams.CreatedAfter.Value);
        }

        if (filterParams.CreatedBefore.HasValue)
        {
            query = query.Where(p => p.CreatedAt <= filterParams.CreatedBefore.Value);
        }

        return query;
    }

    public static IQueryable<Page> ApplySort(this IQueryable<Page> query, string sortBy, bool isDescending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            sortBy = "PageNumber";
        }

        query = sortBy.ToLower() switch
        {
            "pagenumber" => isDescending ? query.OrderByDescending(p => p.PageNumber) : query.OrderBy(p => p.PageNumber),
            "createdat" => isDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            "updatedat" => isDescending ? query.OrderByDescending(p => p.UpdatedAt) : query.OrderBy(p => p.UpdatedAt),
            _ => isDescending ? query.OrderByDescending(p => p.PageNumber) : query.OrderBy(p => p.PageNumber)
        };

        return query;
    }
}
