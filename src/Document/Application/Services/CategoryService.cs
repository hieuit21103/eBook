using Application.DTOs;
using Shared.DTOs;
using Application.DTOs.Category;
using Application.Interfaces;
using Domain.Entities;
using Domain.Filters;
using Domain.Interfaces;

namespace Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<CategoryResponse> GetByIdAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);

        if (category == null)
        {
            throw new KeyNotFoundException($"Category with ID {id} not found.");
        }

        return MapToResponse(category);
    }

    public async Task<PagedResult<CategoryResponse>> GetAllAsync(CategoryFilterParams filterParams)
    {
        var pagedResult = await _categoryRepository.GetPagedAsync(filterParams);

        var categoryResponses = pagedResult.Items.Select(MapToResponse).ToList();

        return new PagedResult<CategoryResponse>
        {
            Items = categoryResponses,
            CurrentPage = pagedResult.CurrentPage,
            PageSize = pagedResult.PageSize,
            TotalPages = pagedResult.TotalPages,
            TotalCount = pagedResult.TotalCount
        };
    }

    public async Task<CategoryResponse> CreateAsync(CategoryCreateRequest request)
    {
        // Check if category with same name exists
        var existing = await _categoryRepository.FindAsync(c => c.Name == request.Name);
        if (existing.Any())
        {
            throw new InvalidOperationException($"Category with name '{request.Name}' already exists.");
        }

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _categoryRepository.AddAsync(category);
        return MapToResponse(result);
    }

    public async Task<CategoryResponse> UpdateAsync(Guid id, CategoryUpdateRequest request)
    {
        var category = await _categoryRepository.GetByIdAsync(id);

        if (category == null)
        {
            throw new KeyNotFoundException($"Category with ID {id} not found.");
        }

        // Check if another category with same name exists
        var existing = await _categoryRepository.FindAsync(c => c.Name == request.Name && c.Id != id);
        if (existing.Any())
        {
            throw new InvalidOperationException($"Category with name '{request.Name}' already exists.");
        }

        category.Name = request.Name;
        category.UpdatedAt = DateTime.UtcNow;

        await _categoryRepository.UpdateAsync(category);
        return MapToResponse(category);
    }

    public async Task DeleteAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);

        if (category == null)
        {
            throw new KeyNotFoundException($"Category with ID {id} not found.");
        }

        await _categoryRepository.DeleteAsync(category);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _categoryRepository.ExistsAsync(c => c.Id == id);
    }

    private static CategoryResponse MapToResponse(Category category)
    {
        return new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }
}
