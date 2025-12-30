using Domain.Entities;
using Shared;

namespace Identity.Test.Services.Users;

public class DeleteAsyncTests : UserServiceBase
{
    [Fact]
    public async Task DeleteAsync_ShouldDeactivateUser_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, IsActive = true };

        _userRepository.GetByIdAsync(userId).Returns(user);

        // Act
        var result = await _userService.DeleteAsync(userId);

        // Assert
        Assert.True(result);
        Assert.False(user.IsActive);
        await _userRepository.Received(1).UpdateAsync(user);
        await _publishEndpoint.Received(1).Publish(Arg.Is<UserStatusChangedEvent>(e =>
            e.UserId == userId && e.IsActive == false));
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenUserNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepository.GetByIdAsync(userId).Returns((User?)null);

        // Act
        var result = await _userService.DeleteAsync(userId);

        // Assert
        Assert.False(result);
        await _userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>());
    }
}
