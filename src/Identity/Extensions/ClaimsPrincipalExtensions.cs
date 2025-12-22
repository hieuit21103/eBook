using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Identity.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string? GetJtiFromClaims(this ClaimsPrincipal user)
    {
        return user.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
    }

    public static string? GetUserIdFromClaims(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public static string? GetEmailFromClaims(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Email)?.Value;
    }

    public static string? GetUsernameFromClaims(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Name)?.Value;
    }

    public static string? GetRoleFromClaims(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Role)?.Value;
    }
}