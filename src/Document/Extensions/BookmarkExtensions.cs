using Domain.Entities;
using Domain.Filters;
using Microsoft.EntityFrameworkCore;

public static class BookmarkExtensions
{
    public static IQueryable<Bookmark> ApplyFilters(this IQueryable<Bookmark> query, BookmarkFilterParams filterParams)
    {
        query = query.Include(b => b.Page)
                     .ThenInclude(p => p.Document);

        if (!string.IsNullOrEmpty(filterParams.Keyword))
        {
            var lowerKeyword = filterParams.Keyword.ToLower();
            query = query.Where(b =>
                b.Page.Document.Title.ToLower().Contains(lowerKeyword) ||
                b.Page.Document.Topic.ToLower().Contains(lowerKeyword)
            );
        }

        if (filterParams.UserId.HasValue)
        {
            query = query.Where(b => b.UserId == filterParams.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filterParams.DocumentTitle))
        {
            var lowerTitle = filterParams.DocumentTitle.ToLower();
            query = query.Where(b => b.Page.Document.Title.ToLower().Contains(lowerTitle));
        }

        if (!string.IsNullOrWhiteSpace(filterParams.DocumentTopic))
        {
            var lowerTopic = filterParams.DocumentTopic.ToLower();
            query = query.Where(b => b.Page.Document.Topic.ToLower().Contains(lowerTopic));
        }

        if (filterParams.CreatedAfter.HasValue)
        {
            query = query.Where(b => b.CreatedAt >= filterParams.CreatedAfter.Value);
        }

        if (filterParams.CreatedBefore.HasValue)
        {
            query = query.Where(b => b.CreatedAt <= filterParams.CreatedBefore.Value);
        }

        return query;
    }

    public static IQueryable<Bookmark> ApplySort(this IQueryable<Bookmark> query, string sortBy, bool isDescending)
    {
        query = query.Include(b => b.Page)
                     .ThenInclude(p => p.Document);

        if (string.IsNullOrWhiteSpace(sortBy.ToString()))
            return query.OrderBy(b => b.CreatedAt);

        return sortBy.ToString().ToLower() switch
        {
            "createdat" => isDescending ? query.OrderByDescending(b => b.CreatedAt) : query.OrderBy(b => b.CreatedAt),
            "documenttitle" => isDescending ? query.OrderByDescending(b => b.Page.Document.Title) : query.OrderBy(b => b.Page.Document.Title),
            _ => query.OrderBy(b => b.CreatedAt)
        };
    }
}