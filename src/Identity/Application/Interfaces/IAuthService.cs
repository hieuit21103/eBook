using Application.DTOs;

namespace Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<AuthResponse?> RefreshTokenAsync(RefreshTokenRequest request);
    Task<bool> LogoutAsync(string jti, string userId);
    Task<bool> RevokeAllTokensAsync(Guid userId);
    Task<bool> IsUserLoggedInAsync(string usernameOrEmail);
}