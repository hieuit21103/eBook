namespace Application.DTOs;

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    Guid UserId,
    string Username,
    string Email,
    string Role
);
