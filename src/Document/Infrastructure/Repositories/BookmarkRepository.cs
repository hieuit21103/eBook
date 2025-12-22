using Domain.Interfaces;
using Domain.Entities;
using Domain.Filters;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Application.DTOs;
using Shared.DTOs;
using Application.DTOs.Bookmark;

namespace Infrastructure.Repositories;

public class BookmarkRepository : Repository<Bookmark>, IBookmarkRepository
{
    public BookmarkRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<PagedResult<Bookmark>> GetPagedAsync(BookmarkFilterParams filter)
    {
        var query = _context.Bookmarks.AsQueryable();

        query = query.ApplySort(filter.SortBy, filter.IsDescending);
        query = query.ApplyFilters(filter);

        var count = await query.CountAsync();

        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(count / (double)filter.PageSize);

        return new PagedResult<Bookmark>()
        {
            Items = items,
            CurrentPage = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalPages = totalPages,
            TotalCount = count
        };
    }
}