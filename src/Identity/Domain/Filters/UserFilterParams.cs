using Domain.Enums;

namespace Domain.Filters;

public class UserFilterParams : FilterParams
{
    public string? Email { get; set; }
    public string? Username { get; set; }
    public Role? Role { get; set; }
    public bool? IsActive { get; set; }
}
