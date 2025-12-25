using Application.DTOs;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Authentication;

namespace Identity.Test.Services.Auth;

public class RefreshTokenAsyncTests : AuthServiceBase
{
    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnNewTokens_WhenTokenIsValid()
    {
        // Arrange
        var request = new RefreshTokenRequest("jti", "refreshToken");
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            Role = Role.User,
            IsActive = true
        };

        _tokenService.ValidateRefreshTokenAsync(request.Jti, request.RefreshToken).Returns(userId);
        _userRepository.GetByIdAsync(userId).Returns(user);
        _tokenService.RefreshAsync(request.Jti, request.RefreshToken, user.Username, user.Email, user.Role.ToString())
            .Returns((userId, "newAccessToken", "newRefreshToken"));

        // Act
        var result = await _authService.RefreshTokenAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("newAccessToken", result.AccessToken);
        Assert.Equal("newRefreshToken", result.RefreshToken);
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldThrowAuthenticationFailureException_WhenTokenIsInvalid()
    {
        // Arrange
        var request = new RefreshTokenRequest("jti", "invalidToken");
        _tokenService.ValidateRefreshTokenAsync(request.Jti, request.RefreshToken).Returns((Guid?)null);

        // Act
        Func<Task> act = async () => await _authService.RefreshTokenAsync(request);

        // Assert
        await act.Should().ThrowAsync<AuthenticationFailureException>()
            .WithMessage("Invalid or expired refresh token.");
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldThrowKeyNotFoundException_WhenUserNotFound()
    {
        // Arrange
        var request = new RefreshTokenRequest("jti", "refreshToken");
        var userId = Guid.NewGuid();

        _tokenService.ValidateRefreshTokenAsync(request.Jti, request.RefreshToken).Returns(userId);
        _userRepository.GetByIdAsync(userId).Returns((User?)null);

        // Act
        Func<Task> act = async () => await _authService.RefreshTokenAsync(request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("User not found or inactive.");
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldThrowAuthenticationFailureException_WhenRefreshFails()
    {
        // Arrange
        var request = new RefreshTokenRequest("jti", "refreshToken");
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            Role = Role.User,
            IsActive = true
        };

        _tokenService.ValidateRefreshTokenAsync(request.Jti, request.RefreshToken).Returns(userId);
        _userRepository.GetByIdAsync(userId).Returns(user);
        _tokenService.RefreshAsync(request.Jti, request.RefreshToken, user.Username, user.Email, user.Role.ToString())
            .Returns(((Guid, string, string)?)null);

        // Act
        Func<Task> act = async () => await _authService.RefreshTokenAsync(request);

        // Assert
        await act.Should().ThrowAsync<AuthenticationFailureException>()
            .WithMessage("Failed to refresh tokens.");
    }
}
