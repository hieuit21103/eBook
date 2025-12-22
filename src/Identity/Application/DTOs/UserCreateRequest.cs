using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class UserCreateRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public string Username { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;

    public Role Role { get; set; } = Role.User;
}
