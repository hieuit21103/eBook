using Domain.Entities;
using Domain.Filters;
using Microsoft.EntityFrameworkCore;

namespace Extensions;

public static class DocumentExtensions
{
    public static IQueryable<Document> ApplyFilters(this IQueryable<Document> query, DocumentFilterParams filterParams)
    {
        query = query.Include(d => d.Pages)
                     .Include(d => d.Categories)
                     .ThenInclude(dc => dc.Category);

        if (!string.IsNullOrEmpty(filterParams.Keyword))
        {
            var lowerKeyword = filterParams.Keyword.ToLower();
            query = query.Where(d =>
                d.Title.ToLower().Contains(lowerKeyword) ||
                d.Topic.ToLower().Contains(lowerKeyword) ||
                (d.Description != null && d.Description.ToLower().Contains(lowerKeyword))
            );
        }

        if (filterParams.UserId.HasValue)
        {
            query = query.Where(d => d.UserId == filterParams.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filterParams.Title))
        {
            var lowerTitle = filterParams.Title.ToLower();
            query = query.Where(d => d.Title.ToLower().Contains(lowerTitle));
        }

        if (!string.IsNullOrWhiteSpace(filterParams.Topic))
        {
            var lowerTopic = filterParams.Topic.ToLower();
            query = query.Where(d => d.Topic.ToLower().Contains(lowerTopic));
        }

        if (filterParams.Categories != null && filterParams.Categories.Any())
        {
            query = query.Where(d => d.Categories.Any(dc => 
                filterParams.Categories.Contains(dc.Category.Name)));
        }

        if (filterParams.CreatedAfter.HasValue)
        {
            query = query.Where(d => d.CreatedAt >= filterParams.CreatedAfter.Value);
        }

        if (filterParams.CreatedBefore.HasValue)
        {
            query = query.Where(d => d.CreatedAt <= filterParams.CreatedBefore.Value);
        }

        // Filter out soft-deleted documents
        query = query.Where(d => !d.IsDeleted);

        return query;
    }

    public static IQueryable<Document> ApplySort(this IQueryable<Document> query, string sortBy, bool isDescending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            sortBy = "CreatedAt";
        }

        query = sortBy.ToLower() switch
        {
            "title" => isDescending ? query.OrderByDescending(d => d.Title) : query.OrderBy(d => d.Title),
            "topic" => isDescending ? query.OrderByDescending(d => d.Topic) : query.OrderBy(d => d.Topic),
            "createdat" => isDescending ? query.OrderByDescending(d => d.CreatedAt) : query.OrderBy(d => d.CreatedAt),
            "updatedat" => isDescending ? query.OrderByDescending(d => d.UpdatedAt) : query.OrderBy(d => d.UpdatedAt),
            "totalpages" => isDescending ? query.OrderByDescending(d => d.TotalPages) : query.OrderBy(d => d.TotalPages),
            _ => isDescending ? query.OrderByDescending(d => d.CreatedAt) : query.OrderBy(d => d.CreatedAt)
        };

        return query;
    }
}
