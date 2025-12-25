using Domain.Entities;

namespace Identity.Test.Services.Auth;

public class LogoutAsyncTests : AuthServiceBase
{
    [Fact]
    public async Task LogoutAsync_ShouldDeleteRefreshToken_WhenValidRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var jti = "jti";
        var user = new User { Id = userId, IsActive = true };

        _userRepository.GetByIdAsync(userId).Returns(user);

        // Act
        await _authService.LogoutAsync(jti, userId.ToString());

        // Assert
        await _tokenService.Received(1).DeleteRefreshTokenAsync(userId.ToString(), jti);
    }

    [Fact]
    public async Task LogoutAsync_ShouldThrowArgumentException_WhenUserIdOrJtiIsMissing()
    {
        // Act
        Func<Task> act = async () => await _authService.LogoutAsync("", "");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Token invalid or missing.");
    }

    [Fact]
    public async Task LogoutAsync_ShouldThrowKeyNotFoundException_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var jti = "jti";

        _userRepository.GetByIdAsync(userId).Returns((User?)null);

        // Act
        Func<Task> act = async () => await _authService.LogoutAsync(jti, userId.ToString());

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("User not found.");
    }
}
