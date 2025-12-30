using Shared.DTOs;
using Shared;

namespace Identity.Test.Services.Users;

public class CreateAsyncTests : UserServiceBase
{
    [Fact]
    public async Task CreateAsync_ShouldReturnUserResponse_WhenRequestIsValid()
    {
        // Arrange
        var request = new UserCreateRequest
        {
            Username = "newuser",
            Email = "new@test.com",
            Password = "password123",
            Role = Role.User
        };

        _userRepository.ExistsByEmailAsync(request.Email).Returns(false);
        _userRepository.ExistsByUsernameAsync(request.Username).Returns(false);
        _passwordService.HashPassword(request.Password).Returns("hashed_password");

        // Act
        var result = await _userService.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Username, result.Username);
        Assert.Equal(request.Email, result.Email);

        await _userRepository.Received(1).AddAsync(Arg.Is<User>(u =>
            u.Username == request.Username &&
            u.Email == request.Email &&
            u.Password == "hashed_password"
        ));
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowException_WhenEmailExists()
    {
        // Arrange
        var request = new UserCreateRequest { Email = "existing@test.com" };
        _userRepository.ExistsByEmailAsync(request.Email).Returns(true);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _userService.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowException_WhenUsernameExists()
    {
        // Arrange
        var request = new UserCreateRequest { Username = "existinguser", Email = "new@test.com" };
        _userRepository.ExistsByEmailAsync(request.Email).Returns(false);
        _userRepository.ExistsByUsernameAsync(request.Username).Returns(true);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _userService.CreateAsync(request));
    }
}
