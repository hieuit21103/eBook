using Application.DTOs;
using Shared.DTOs;
using Domain.Filters;

namespace Application.Interfaces;

public interface IUserService
{
    Task<PagedResult<UserResponse>> GetAllAsync(UserFilterParams filterParams);
    Task<UserResponse?> GetByIdAsync(Guid id);
    Task<UserResponse> CreateAsync(UserCreateRequest request);
    Task<UserResponse?> UpdateAsync(Guid id, UserUpdateRequest request);
    Task<bool> DeleteAsync(Guid id);
}
