using Application.DTOs;
using Shared.DTOs;
using Domain.Entities;
using Domain.Filters;

namespace Domain.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<PagedResult<Category>> GetPagedAsync(CategoryFilterParams filterParams);
}
