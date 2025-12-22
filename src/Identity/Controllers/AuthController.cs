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
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RegisterAsync(request);

            if (result == null)
            {
                return BadRequest(new { message = "Email or username already exists" });
            }

            HttpContext.Response.Cookies.Append("refreshToken", result.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            return Ok(new { accessToken = result.AccessToken, userId = result.UserId, username = result.Username, email = result.Email, role = result.Role });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return StatusCode(500, new { message = "An error occurred during registration" });
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if(HttpContext.Request.Cookies.TryGetValue("refreshToken", out var existingRefreshToken))
            {
                return BadRequest(new { message = "User is already logged in" });
            }

            var result = await _authService.LoginAsync(request);

            if (result == null)
            {
                return Unauthorized(new { message = "Invalid credentials or account is inactive" });
            }

            HttpContext.Response.Cookies.Append("refreshToken", result.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            return Ok(new { accessToken = result.AccessToken, userId = result.UserId, username = result.Username, email = result.Email, role = result.Role });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken()
    {
        try
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

            if (result == null)
            {
                return Unauthorized(new { message = "Invalid or expired refresh token" });
            }

            HttpContext.Response.Cookies.Append("refreshToken", result.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, new { message = "An error occurred during token refresh" });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var jtiClaim = User.GetJtiFromClaims();
            var userIdClaim = User.GetUserIdFromClaims();

            if (string.IsNullOrEmpty(jtiClaim))
            {
                return BadRequest(new { message = "Invalid token claims" });
            }

            var result = await _authService.LogoutAsync(jtiClaim, userIdClaim);

            if (!result)
            {
                return BadRequest(new { message = "Logout failed" });
            }

            HttpContext.Response.Cookies.Delete("refreshToken");

            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new { message = "An error occurred during logout" });
        }
    }

    [HttpPost("revoke-all-tokens")]
    [Authorize]
    public async Task<IActionResult> RevokeAllTokens()
    {
        try
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

            var result = await _authService.RevokeAllTokensAsync(userId);

            if (!result)
            {
                return BadRequest(new { message = "Revoke tokens failed" });
            }

            return Ok(new { message = "All tokens revoked successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token revocation");
            return StatusCode(500, new { message = "An error occurred during token revocation" });
        }
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }
}
