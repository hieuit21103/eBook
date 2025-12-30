using System.Security.Claims;

public static class ClaimsExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var idClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(idClaim, out var userId))
        {
            return userId;
        }
        throw new UnauthorizedAccessException("Invalid User ID in Token");
    }

    public static string GetUserRole(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.Role) ?? "User";
    }

    public static string GetUserName(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.Name) ?? "Unknown";
    }
}