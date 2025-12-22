using Domain.Enums;

namespace Application.DTOs;

public class UserUpdateRequest
{
    public string? Email { get; set; }
    public string? Username { get; set; }
    public Role? Role { get; set; }
    public bool? IsActive { get; set; }
    public string? Password { get; set; }
}
