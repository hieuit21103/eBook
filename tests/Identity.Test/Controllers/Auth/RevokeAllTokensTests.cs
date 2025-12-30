using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Identity.Test.Controllers.Auth;

public class RevokeAllTokensTests : AuthControllerBase
{
    [Fact]
    public async Task RevokeAllTokens_ShouldReturnOk_WhenUserIdIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };
        _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = await _controller.RevokeAllTokens();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        await _authService.Received(1).RevokeAllTokensAsync(userId);
    }

    [Fact]
    public async Task RevokeAllTokens_ShouldReturnBadRequest_WhenUserIdClaimIsMissing()
    {
        // Arrange
        _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var result = await _controller.RevokeAllTokens();

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task RevokeAllTokens_ShouldReturnBadRequest_WhenUserIdIsInvalid()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "invalid-guid")
        };
        _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = await _controller.RevokeAllTokens();

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}
