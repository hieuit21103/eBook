namespace Identity.Test.Services.Users;

public class GetByIdAsyncTests : UserServiceBase
{
    [Fact]
    public async Task GetByIdAsync_ShouldReturnUserResponse_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            Username = "testuser",
            Role = Role.User,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _userRepository.GetByIdAsync(userId).Returns(user);

        // Act
        var result = await _userService.GetByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.Email.Should().Be(user.Email);
        result.Username.Should().Be(user.Username);
        result.Role.Should().Be(user.Role);
        result.IsActive.Should().Be(user.IsActive);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepository.GetByIdAsync(userId).Returns((User?)null);

        // Act
        var result = await _userService.GetByIdAsync(userId);

        // Assert
        result.Should().BeNull();
    }
}