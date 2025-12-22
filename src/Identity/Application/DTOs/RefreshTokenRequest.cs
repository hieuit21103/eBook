namespace Application.DTOs;

public record RefreshTokenRequest(
    string Jti,
    string RefreshToken
);
