using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Identity.Test.Controllers.Auth;

public class RefreshTokenTests : AuthControllerBase
{
    [Fact]
    public async Task RefreshToken_ShouldReturnOk_WhenTokensAreValid()
    {
        // Arrange
        var jti = "test_jti";
        var refreshToken = "test_refresh_token";

        // Create a dummy JWT with JTI
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateJwtSecurityToken(subject: new ClaimsIdentity(new[] { new Claim(JwtRegisteredClaimNames.Jti, jti) }));
        var tokenString = tokenHandler.WriteToken(token);

        _httpContext.Request.Headers["Authorization"] = $"Bearer {tokenString}";

        // Setup Cookie for Refresh Token
        _httpContext.Request.Headers["Cookie"] = $"refreshToken={refreshToken}";

        var authResponse = new AuthResponse("newAccessToken", "newRefreshToken", Guid.NewGuid(), "testuser", "test@example.com", "User");
        _authService.RefreshTokenAsync(Arg.Any<RefreshTokenRequest>()).Returns(authResponse);

        // Act
        var result = await _controller.RefreshToken();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnBadRequest_WhenJtiIsMissing()
    {
        // Arrange
        // No JTI claim

        // Act
        var result = await _controller.RefreshToken();

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task RefreshToken_ShouldReturnBadRequest_WhenRefreshTokenCookieIsMissing()
    {
        // Arrange
        var jti = "test_jti";
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateJwtSecurityToken(subject: new ClaimsIdentity(new[] { new Claim(JwtRegisteredClaimNames.Jti, jti) }));
        var tokenString = tokenHandler.WriteToken(token);

        _httpContext.Request.Headers["Authorization"] = $"Bearer {tokenString}";

        // No Cookie

        // Act
        var result = await _controller.RefreshToken();

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}
