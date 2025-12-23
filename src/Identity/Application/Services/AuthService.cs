using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IPasswordService _passwordService;

    public AuthService(
        IUserRepository userRepository, 
        ITokenService tokenService, 
        IPasswordService passwordService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _passwordService = passwordService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _userRepository.ExistsByEmailAsync(request.Email))
        {
            throw new ArgumentException("Email is already in use.");
        }

        if (await _userRepository.ExistsByUsernameAsync(request.Username))
        {
            throw new ArgumentException("Username is already in use.");
        }

        var passwordHash = _passwordService.HashPassword(request.Password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            Username = request.Username,
            Password = passwordHash,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);

        var accessToken = _tokenService.GenerateAccessToken(
            user.Id,
            user.Username,
            user.Email,
            user.Role.ToString()
        );

        var jti = ExtractJtiFromToken(accessToken);
        
        var refreshToken = _tokenService.GenerateRefreshToken(user.Id);
        await _tokenService.StoreRefreshTokenAsync(user.Id, jti, refreshToken);

        return new AuthResponse(
            accessToken,
            refreshToken,
            user.Id,
            user.Username,
            user.Email,
            user.Role.ToString()
        );
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        User? user = null;

        if (request.UsernameOrEmail.Contains('@'))
        {
            user = await _userRepository.GetByEmailAsync(request.UsernameOrEmail);
        }
        else
        {
            user = await _userRepository.GetByUsernameAsync(request.UsernameOrEmail);
        }

        if (user == null)
        {
            throw new KeyNotFoundException("No user found with the provided username or email.");
        }

        if(!_passwordService.VerifyPassword(request.Password, user.Password))
        {
            throw new AuthenticationFailureException("Invalid password.");
        }

        if (!user.IsActive)
        {
            throw new InvalidOperationException("User account is not active.");
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        await _tokenService.DeleteAllUserTokensAsync(user.Id);

        var accessToken = _tokenService.GenerateAccessToken(
            user.Id,
            user.Username,
            user.Email,
            user.Role.ToString()
        );

        var jti = ExtractJtiFromToken(accessToken);
        
        var refreshToken = _tokenService.GenerateRefreshToken(user.Id);
        await _tokenService.StoreRefreshTokenAsync(user.Id, jti, refreshToken);

        return new AuthResponse(
            accessToken,
            refreshToken,
            user.Id,
            user.Username,
            user.Email,
            user.Role.ToString()
        );
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var userId = await _tokenService.ValidateRefreshTokenAsync(request.Jti, request.RefreshToken);
        if (userId == null)
        {
            throw new AuthenticationFailureException("Invalid or expired refresh token.");
        }

        var user = await _userRepository.GetByIdAsync(userId.Value);
        if (user == null || !user.IsActive)
        {
            throw new KeyNotFoundException("User not found or inactive.");
        }

        var newTokens = await _tokenService.RefreshAsync(
            request.Jti,
            request.RefreshToken,
            user.Username,
            user.Email,
            user.Role.ToString()
        );

        if (newTokens == null)
        {
            throw new AuthenticationFailureException("Failed to refresh tokens.");
        }

        return new AuthResponse(
            newTokens.Value.accessToken,
            newTokens.Value.refreshToken,
            user.Id,
            user.Username,
            user.Email,
            user.Role.ToString()
        );
    }

    public async Task LogoutAsync(string jti, string userId)
    {
        if(string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(jti))
        {
            throw new ArgumentException("Token invalid or missing.");
        }

        var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
        if (user == null || !user.IsActive)
        {
            throw new KeyNotFoundException("User not found.");
        }
        
        await _tokenService.DeleteRefreshTokenAsync(userId, jti);
    }

    public async Task RevokeAllTokensAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        await _tokenService.DeleteAllUserTokensAsync(userId);
    }

    private string ExtractJtiFromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var jtiClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);
        return jtiClaim?.Value ?? Guid.NewGuid().ToString();
    }
}
