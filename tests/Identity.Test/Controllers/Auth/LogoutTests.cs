using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Identity.Test.Controllers.Auth;

public class LogoutTests : AuthControllerBase
{
    [Fact]
    public async Task Logout_ShouldReturnOk_WhenClaimsAreValid()
    {
        // Arrange
        var jti = "test-jti";
        var userId = "test-user-id";
        var claims = new List<Claim>
        {
            new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, jti),
            new Claim(ClaimTypes.NameIdentifier, userId)
        };
        _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = await _controller.Logout();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        await _authService.Received(1).LogoutAsync(jti, userId);
    }

    [Fact]
    public async Task Logout_ShouldReturnBadRequest_WhenJtiIsMissing()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user-id")
        };
        _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = await _controller.Logout();

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}
