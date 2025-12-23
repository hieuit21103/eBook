namespace Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(Guid userId, string username, string email, string role, string jti = "");
    string GenerateRefreshToken(Guid userId);
    bool ValidateAccessToken(string token);
    Task<Guid?> ValidateRefreshTokenAsync(
        string jti,
        string refreshToken);
    Task StoreRefreshTokenAsync(
        Guid userId,
        string jti,
        string refreshToken);
    Task<(Guid userId, string accessToken, string refreshToken)?> RefreshAsync(
        string oldJti,
        string refreshToken,
        string username,
        string email,
        string role);
    Task DeleteRefreshTokenAsync(string userId, string jti);
    Task DeleteAllUserTokensAsync(Guid userId);
}