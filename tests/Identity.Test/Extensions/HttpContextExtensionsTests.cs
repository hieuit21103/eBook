using FluentAssertions;
using Identity.Extensions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Identity.Test.Extensions;

public class HttpContextExtensionsTests
{
    [Fact]
    public void GetRefreshTokenFromCookie_ShouldReturnToken_WhenCookieExists()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var token = "test-refresh-token";
        context.Request.Headers["Cookie"] = $"refreshToken={token}";

        // Act
        var result = context.GetRefreshTokenFromCookie();

        // Assert
        result.Should().Be(token);
    }

    [Fact]
    public void GetRefreshTokenFromCookie_ShouldReturnNull_WhenCookieDoesNotExist()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act
        var result = context.GetRefreshTokenFromCookie();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetJtiFromAccessToken_ShouldReturnJti_WhenTokenIsValid()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var jti = "test-jti";
        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var token = tokenHandler.CreateJwtSecurityToken(subject: new System.Security.Claims.ClaimsIdentity(new[] 
        { 
            new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, jti) 
        }));
        var tokenString = tokenHandler.WriteToken(token);
        context.Request.Headers["Authorization"] = $"Bearer {tokenString}";

        // Act
        var result = context.GetJtiFromAccessToken();

        // Assert
        result.Should().Be(jti);
    }

    [Fact]
    public void GetJtiFromAccessToken_ShouldReturnNull_WhenTokenIsMissing()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act
        var result = context.GetJtiFromAccessToken();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetJtiFromAccessToken_ShouldReturnNull_WhenTokenIsInvalid()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["Authorization"] = "Bearer invalid-token";

        // Act
        var result = context.GetJtiFromAccessToken();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetUserIdFromAccessToken_ShouldReturnUserId_WhenTokenIsValid()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var userId = "test-user-id";
        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var token = tokenHandler.CreateJwtSecurityToken(subject: new System.Security.Claims.ClaimsIdentity(new[] 
        { 
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId) 
        }));
        var tokenString = tokenHandler.WriteToken(token);
        context.Request.Headers["Authorization"] = $"Bearer {tokenString}";

        // Act
        var result = context.GetUserIdFromAccessToken();

        // Assert
        result.Should().Be(userId);
    }

    [Fact]
    public void GetUserIdFromAccessToken_ShouldReturnNull_WhenTokenIsMissing()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act
        var result = context.GetUserIdFromAccessToken();

        // Assert
        result.Should().BeNull();
    }
}
