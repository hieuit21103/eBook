using Domain.Entities;
using Domain.Interfaces;
using Domain.Filters;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Application.DTOs;
using Shared.DTOs;
using Extensions;

namespace Infrastructure.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<PagedResult<Category>> GetPagedAsync(CategoryFilterParams filter)
    {
        var query = _context.Categories.AsQueryable();

        // Apply sorting and filtering
        query = query.ApplySort(filter.SortBy, filter.IsDescending);
        query = query.ApplyFilters(filter);

        // Get total count before pagination
        var count = await query.CountAsync();

        // Apply pagination and fetch data
        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(count / (double)filter.PageSize);

        return new PagedResult<Category>()
        {
            Items = items,
            CurrentPage = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalPages = totalPages,
            TotalCount = count
        };
    }
}
