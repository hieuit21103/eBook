using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Shared.DTOs;
using Shared;
using Domain.Filters;
using MassTransit;

namespace Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository, 
        IPasswordService passwordService, 
        IPublishEndpoint publishEndpoint, 
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<PagedResult<UserResponse>> GetAllAsync(UserFilterParams filterParams)
    {
        var pagedUsers = await _userRepository.GetAllAsync(filterParams);
        
        var userResponses = pagedUsers.Items.Select(u => new UserResponse
        {
            Id = u.Id,
            Email = u.Email,
            Username = u.Username,
            Role = u.Role,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt
        });

        return new PagedResult<UserResponse>
        {
            Items = userResponses,
            CurrentPage = pagedUsers.CurrentPage,
            PageSize = pagedUsers.PageSize,
            TotalCount = pagedUsers.TotalCount,
            TotalPages = pagedUsers.TotalPages
        };
    }

    public async Task<UserResponse?> GetByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return null;

        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<UserResponse> CreateAsync(UserCreateRequest request)
    {
        if (await _userRepository.ExistsByEmailAsync(request.Email))
        {
            throw new Exception("Email already exists");
        }

        if (await _userRepository.ExistsByUsernameAsync(request.Username))
        {
            throw new Exception("Username already exists");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            Username = request.Username,
            Password = _passwordService.HashPassword(request.Password),
            Role = request.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);

        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<UserResponse?> UpdateAsync(Guid id, UserUpdateRequest request)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return null;

        if (!string.IsNullOrEmpty(request.Email) && user.Email != request.Email)
        {
             if (await _userRepository.ExistsByEmailAsync(request.Email))
            {
                throw new Exception("Email already exists");
            }
            user.Email = request.Email;
        }

        if (!string.IsNullOrEmpty(request.Username) && user.Username != request.Username)
        {
             if (await _userRepository.ExistsByUsernameAsync(request.Username))
            {
                throw new Exception("Username already exists");
            }
            user.Username = request.Username;
        }

        if (request.Role.HasValue)
        {
            user.Role = request.Role.Value;
        }

        if (request.IsActive.HasValue && user.IsActive != request.IsActive.Value)
        {
            user.IsActive = request.IsActive.Value;
        }

        if (!string.IsNullOrEmpty(request.Password))
        {
            user.Password = _passwordService.HashPassword(request.Password);
        }

        await _userRepository.UpdateAsync(user);

        if (request.IsActive.HasValue && user.IsActive != request.IsActive.Value)
        {
            await _publishEndpoint.Publish(new UserStatusChangedEvent
            {
                UserId = user.Id,
                IsActive = user.IsActive
            });
        }

        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return false;
        
        user.IsActive = false;
        await _userRepository.UpdateAsync(user);

        await _publishEndpoint.Publish(new UserStatusChangedEvent
        {
            UserId = user.Id,
            IsActive = user.IsActive
        });
        return true;
    }
}
