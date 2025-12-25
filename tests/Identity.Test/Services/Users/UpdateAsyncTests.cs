using Application.DTOs;
using Domain.Entities;
using Domain.Enums;
using Shared;

namespace Identity.Test.Services.Users;

public class UpdateAsyncTests : UserServiceBase
{
    [Fact]
    public async Task UpdateAsync_ShouldUpdateUser_WhenValidRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "old@example.com",
            Username = "olduser",
            Role = Role.User,
            IsActive = true
        };

        var request = new UserUpdateRequest
        {
            Email = "new@example.com",
            Username = "newuser",
            Role = Role.Admin,
            IsActive = false,
            Password = "newpassword"
        };

        _userRepository.GetByIdAsync(userId).Returns(user);
        _userRepository.ExistsByEmailAsync(request.Email).Returns(false);
        _userRepository.ExistsByUsernameAsync(request.Username).Returns(false);
        _passwordService.HashPassword(request.Password).Returns("hashedNewPassword");

        // Act
        var result = await _userService.UpdateAsync(userId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Email, result.Email);
        Assert.Equal(request.Username, result.Username);
        Assert.Equal(request.Role, result.Role);
        Assert.Equal(request.IsActive, result.IsActive);
        
        await _userRepository.Received(1).UpdateAsync(user);
        await _publishEndpoint.Received(1).Publish(Arg.Is<UserStatusChangedEvent>(e => 
            e.UserId == userId && e.IsActive == false));
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowException_WhenEmailExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Email = "old@example.com" };
        var request = new UserUpdateRequest { Email = "existing@example.com" };

        _userRepository.GetByIdAsync(userId).Returns(user);
        _userRepository.ExistsByEmailAsync(request.Email).Returns(true);

        // Act
        Func<Task> act = async () => await _userService.UpdateAsync(userId, request);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Email already exists");
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowException_WhenUsernameExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Username = "olduser" };
        var request = new UserUpdateRequest { Username = "existinguser" };

        _userRepository.GetByIdAsync(userId).Returns(user);
        _userRepository.ExistsByUsernameAsync(request.Username).Returns(true);

        // Act
        Func<Task> act = async () => await _userService.UpdateAsync(userId, request);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Username already exists");
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNull_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UserUpdateRequest();
        _userRepository.GetByIdAsync(userId).Returns((User?)null);

        // Act
        var result = await _userService.UpdateAsync(userId, request);

        // Assert
        Assert.Null(result);
    }
}
