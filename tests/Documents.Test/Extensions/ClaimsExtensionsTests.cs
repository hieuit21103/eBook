using System.Security.Claims;
using FluentAssertions;
using Xunit;

namespace Documents.Test.Extensions;

public class ClaimsExtensionsTests
{
    [Fact]
    public void GetUserId_ShouldReturnUserId_WhenClaimExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = user.GetUserId();

        // Assert
        result.Should().Be(userId);
    }

    [Fact]
    public void GetUserId_ShouldThrowUnauthorizedAccessException_WhenClaimIsInvalid()
    {
        // Arrange
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "invalid-guid") };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        Action act = () => user.GetUserId();

        // Assert
        act.Should().Throw<UnauthorizedAccessException>().WithMessage("Invalid User ID in Token");
    }

    [Fact]
    public void GetUserRole_ShouldReturnRole_WhenClaimExists()
    {
        // Arrange
        var role = "Admin";
        var claims = new List<Claim> { new Claim(ClaimTypes.Role, role) };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims));

        // Act
        var result = user.GetUserRole();

        // Assert
        result.Should().Be(role);
    }

    [Fact]
    public void GetUserRole_ShouldReturnDefaultUser_WhenClaimDoesNotExist()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var result = user.GetUserRole();

        // Assert
        result.Should().Be("User");
    }
}
