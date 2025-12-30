using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Identity.Test.Controllers.Auth;

public class GetCurrentUserTests : AuthControllerBase
{
    [Fact]
    public void GetCurrentUser_ShouldReturnOk_WhenClaimsAreValid()
    {
        // Arrange
        var userId = "test-user-id";
        var username = "testuser";
        var email = "test@example.com";
        var role = "User";

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role)
        };
        _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = _controller.GetCurrentUser();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        // We can't easily check properties of anonymous object without reflection or dynamic, 
        // but checking it's OK is a good start.
    }

    [Fact]
    public void GetCurrentUser_ShouldReturnUnauthorized_WhenUserIdClaimIsMissing()
    {
        // Arrange
        _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var result = _controller.GetCurrentUser();

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }
}
