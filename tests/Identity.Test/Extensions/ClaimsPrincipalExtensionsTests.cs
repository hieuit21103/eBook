using System.Security.Claims;
using FluentAssertions;
using Identity.Extensions;
using Xunit;

namespace Identity.Test.Extensions;

public class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void GetJtiFromClaims_ShouldReturnJti_WhenClaimExists()
    {
        // Arrange
        var jti = "test-jti";
        var claims = new List<Claim> { new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, jti) };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = user.GetJtiFromClaims();

        // Assert
        result.Should().Be(jti);
    }

    [Fact]
    public void GetUserIdFromClaims_ShouldReturnUserId_WhenClaimExists()
    {
        // Arrange
        var userId = "test-user-id";
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = user.GetUserIdFromClaims();

        // Assert
        result.Should().Be(userId);
    }

    [Fact]
    public void GetEmailFromClaims_ShouldReturnEmail_WhenClaimExists()
    {
        // Arrange
        var email = "test@example.com";
        var claims = new List<Claim> { new Claim(ClaimTypes.Email, email) };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = user.GetEmailFromClaims();

        // Assert
        result.Should().Be(email);
    }

    [Fact]
    public void GetUsernameFromClaims_ShouldReturnUsername_WhenClaimExists()
    {
        // Arrange
        var username = "testuser";
        var claims = new List<Claim> { new Claim(ClaimTypes.Name, username) };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = user.GetUsernameFromClaims();

        // Assert
        result.Should().Be(username);
    }

    [Fact]
    public void GetRoleFromClaims_ShouldReturnRole_WhenClaimExists()
    {
        // Arrange
        var role = "Admin";
        var claims = new List<Claim> { new Claim(ClaimTypes.Role, role) };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = user.GetRoleFromClaims();

        // Assert
        result.Should().Be(role);
    }
}
