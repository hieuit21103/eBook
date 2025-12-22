using Domain.Entities;
using Shared.DTOs;
using Domain.Filters;

namespace Domain.Interfaces;

public interface IUserRepository
{
    Task<PagedResult<User>> GetAllAsync(UserFilterParams filterParams);
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task<int> CountAsync();
    Task<bool> ExistsByEmailAsync(string email);
    Task<bool> ExistsByUsernameAsync(string username);
}