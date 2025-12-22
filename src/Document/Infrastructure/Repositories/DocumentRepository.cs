using Domain.Entities;
using Domain.Interfaces;
using Domain.Filters;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Application.DTOs;
using Shared.DTOs;
using Extensions;

namespace Infrastructure.Repositories;

public class DocumentRepository : Repository<Document>, IDocumentRepository
{
    public DocumentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<PagedResult<Document>> GetPagedAsync(DocumentFilterParams filter)
    {
        var query = _context.Documents.AsQueryable();

        // Apply sorting and filtering
        query = query.ApplySort(filter.SortBy, filter.IsDescending);
        query = query.ApplyFilters(filter);

        // Include related entities
        query = query.Include(d => d.Categories).ThenInclude(dc => dc.Category);

        // Get total count before pagination
        var count = await query.CountAsync();

        // Apply pagination and fetch data
        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(count / (double)filter.PageSize);

        return new PagedResult<Document>()
        {
            Items = items,
            CurrentPage = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalPages = totalPages,
            TotalCount = count
        };
    }

    public async Task<Document?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _context.Documents
            .Include(d => d.Pages)
            .Include(d => d.Categories)
            .ThenInclude(dc => dc.Category)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);
    }

    public async Task<IEnumerable<Document>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Documents
            .Where(d => d.UserId == userId && !d.IsDeleted)
            .ToListAsync();
    }
}
