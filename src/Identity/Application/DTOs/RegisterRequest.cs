namespace Application.DTOs;

public record RegisterRequest(
    string Email,
    string Username,
    string Password
);
