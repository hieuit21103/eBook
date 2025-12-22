using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Application.DTOs;
using Shared.DTOs;
using Domain.Filters;
using Extensions;

namespace Infrastructure.Repositories;

public class PageRepository : Repository<Page>, IPageRepository
{
    public PageRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<PagedResult<Page>> GetPagedAsync(PageFilterParams filter)
    {
        var query = _context.Pages.AsQueryable();

        query = query.ApplySort(filter.SortBy, filter.IsDescending);
        query = query.ApplyFilters(filter);

        var count = await query.CountAsync();

        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(count / (double)filter.PageSize);

        return new PagedResult<Page>()
        {
            Items = items,
            CurrentPage = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalPages = totalPages,
            TotalCount = count
        };
    }

    public async Task<IEnumerable<Page>> GetPagesByDocumentIdAsync(Guid documentId)
    {
        return await _context.Pages
            .Include(p => p.Document)
            .Where(p => p.DocumentId == documentId)
            .OrderBy(p => p.PageNumber)
            .ToListAsync();
    }

    public async Task<Page?> GetByDocumentIdAndPageNumberAsync(Guid documentId, int pageNumber)
    {
        return await _context.Pages
            .Include(p => p.Document)
            .FirstOrDefaultAsync(p => p.DocumentId == documentId && p.PageNumber == pageNumber);
    }

    public async Task<IEnumerable<Page>> GetPagesGreaterThanOrEqualToAsync(Guid documentId, int pageNumber)
    {
        return await _context.Pages
            .Where(p => p.DocumentId == documentId && p.PageNumber >= pageNumber)
            .OrderBy(p => p.PageNumber)
            .ToListAsync();
    }

    public async Task<Page?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _context.Pages
            .Include(p => p.Document)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
}
