using Application.DTOs;
using Shared.DTOs;
using Application.DTOs.Category;
using Domain.Filters;

namespace Application.Interfaces;

public interface ICategoryService
{
    Task<CategoryResponse> GetByIdAsync(Guid id);
    Task<PagedResult<CategoryResponse>> GetAllAsync(CategoryFilterParams filterParams);
    Task<CategoryResponse> CreateAsync(CategoryCreateRequest request);
    Task<CategoryResponse> UpdateAsync(Guid id, CategoryUpdateRequest request);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}
