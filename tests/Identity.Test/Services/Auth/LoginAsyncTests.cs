using Application.DTOs;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Authentication;

namespace Identity.Test.Services.Auth;

public class LoginAsyncTests : AuthServiceBase
{
    [Fact]
    public async Task LoginAsync_ShouldReturnAuthResponse_WhenCredentialsAreValid_WithEmail()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "password123");
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.UsernameOrEmail,
            Username = "testuser",
            Password = "hashedPassword",
            IsActive = true,
            Role = Role.User
        };

        _userRepository.GetByEmailAsync(request.UsernameOrEmail).Returns(user);
        _passwordService.VerifyPassword(request.Password, user.Password).Returns(true);
        
        // Create a dummy JWT
        var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyLCJqdGkiOiJqdGkifQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
        
        _tokenService.GenerateAccessToken(user.Id, user.Username, user.Email, user.Role.ToString())
            .Returns(token);
        _tokenService.GenerateRefreshToken(user.Id).Returns("refreshToken");

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(token, result.AccessToken);
        Assert.Equal("refreshToken", result.RefreshToken);
        await _tokenService.Received(1).DeleteAllUserTokensAsync(user.Id);
        await _tokenService.Received(1).StoreRefreshTokenAsync(user.Id, Arg.Any<string>(), "refreshToken");
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnAuthResponse_WhenCredentialsAreValid_WithUsername()
    {
        // Arrange
        var request = new LoginRequest("testuser", "password123");
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Username = request.UsernameOrEmail,
            Password = "hashedPassword",
            IsActive = true,
            Role = Role.User
        };

        _userRepository.GetByUsernameAsync(request.UsernameOrEmail).Returns(user);
        _passwordService.VerifyPassword(request.Password, user.Password).Returns(true);
        
        // Create a dummy JWT
        var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyLCJqdGkiOiJqdGkifQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";

        _tokenService.GenerateAccessToken(user.Id, user.Username, user.Email, user.Role.ToString())
            .Returns(token);
        _tokenService.GenerateRefreshToken(user.Id).Returns("refreshToken");

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(token, result.AccessToken);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowKeyNotFoundException_WhenUserNotFound()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "password123");
        _userRepository.GetByEmailAsync(request.UsernameOrEmail).Returns((User?)null);

        // Act
        Func<Task> act = async () => await _authService.LoginAsync(request);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("No user found with the provided username or email.");
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowAuthenticationFailureException_WhenPasswordIsInvalid()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "wrongpassword");
        var user = new User
        {
            Email = request.UsernameOrEmail,
            Password = "hashedPassword"
        };

        _userRepository.GetByEmailAsync(request.UsernameOrEmail).Returns(user);
        _passwordService.VerifyPassword(request.Password, user.Password).Returns(false);

        // Act
        Func<Task> act = async () => await _authService.LoginAsync(request);

        // Assert
        await act.Should().ThrowAsync<AuthenticationFailureException>()
            .WithMessage("Invalid password.");
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowInvalidOperationException_WhenUserIsInactive()
    {
        // Arrange
        var request = new LoginRequest("test@example.com", "password123");
        var user = new User
        {
            Email = request.UsernameOrEmail,
            Password = "hashedPassword",
            IsActive = false
        };

        _userRepository.GetByEmailAsync(request.UsernameOrEmail).Returns(user);
        _passwordService.VerifyPassword(request.Password, user.Password).Returns(true);

        // Act
        Func<Task> act = async () => await _authService.LoginAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User account is not active.");
    }
}
