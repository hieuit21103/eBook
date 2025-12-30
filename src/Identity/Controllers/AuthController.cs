using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Identity.Extensions;

namespace Identity.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.RegisterAsync(request);

        HttpContext.Response.Cookies.Append("refreshToken", result.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });

        return Ok(new
        {
            accessToken = result.AccessToken,
            userId = result.UserId,
            username = result.Username,
            email = result.Email,
            role = result.Role
        });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.LoginAsync(request);

        HttpContext.Response.Cookies.Append("refreshToken", result.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });

        return Ok(new
        {
            accessToken = result.AccessToken,
            userId = result.UserId,
            username = result.Username,
            email = result.Email,
            role = result.Role
        });
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken()
    {
        var jti = HttpContext.GetJtiFromAccessToken();
        if (string.IsNullOrEmpty(jti))
        {
            return BadRequest(new { message = "Invalid or missing access token" });
        }

        var refreshToken = HttpContext.GetRefreshTokenFromCookie();
        if (string.IsNullOrEmpty(refreshToken))
        {
            return BadRequest(new { message = "Refresh token not found in cookies" });
        }

        var request = new RefreshTokenRequest(jti, refreshToken);
        var result = await _authService.RefreshTokenAsync(request);

        HttpContext.Response.Cookies.Append("refreshToken", result.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });

        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var jtiClaim = User.GetJtiFromClaims();
        var userIdClaim = User.GetUserIdFromClaims();

        if (string.IsNullOrEmpty(jtiClaim))
        {
            return BadRequest(new { message = "Invalid token claims" });
        }

        await _authService.LogoutAsync(jtiClaim, userIdClaim);

        HttpContext.Response.Cookies.Delete("refreshToken");

        return Ok(new
        {
            message = "Logged out successfully"
        });
    }

    [HttpPost("revoke-all-tokens")]
    [Authorize]
    public async Task<IActionResult> RevokeAllTokens()
    {
        var userIdClaim = User.GetUserIdFromClaims();

        if (string.IsNullOrEmpty(userIdClaim))
        {
            return BadRequest(new { message = "Invalid token claims" });
        }

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return BadRequest(new { message = "Invalid user ID" });
        }

        await _authService.RevokeAllTokensAsync(userId);

        return Ok(new { message = "All tokens revoked successfully" });
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        var userIdClaim = User.GetUserIdFromClaims();
        var usernameClaim = User.GetUsernameFromClaims();
        var emailClaim = User.GetEmailFromClaims();
        var roleClaim = User.GetRoleFromClaims();

        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized(new { message = "Invalid token" });
        }

        return Ok(new
        {
            userId = userIdClaim,
            username = usernameClaim,
            email = emailClaim,
            role = roleClaim
        });
    }
}