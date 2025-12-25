using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Domain.Entities;

namespace Identity.Test.Services.Auth;

public class RegisterAsyncTests : AuthServiceBase
{
    [Fact]
    public async Task RegisterAsync_ShouldReturnAuthResponse_WhenValidRequest()
    {
        // Arrange
        var request = new RegisterRequest(
            Email: "test@example.com",
            Username: "testuser",
            Password: "password123"
        );

        var userId = Guid.NewGuid();
        var refreshToken = "refresh_token";
        Domain.Entities.User? addedUser = null;

        // Create a valid JWT token with JTI
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("test-secret-key-for-jwt-signing-32bytes"));
        var jti = Guid.NewGuid().ToString();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, request.Username),
                new Claim(ClaimTypes.Email, request.Email),
                new Claim(ClaimTypes.Role, "User"),
                new Claim(JwtRegisteredClaimNames.Jti, jti)
            ]),
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var accessToken = tokenHandler.WriteToken(token);

        _userRepository.ExistsByEmailAsync(request.Email).Returns(false);
        _userRepository.ExistsByUsernameAsync(request.Username).Returns(false);
        _userRepository.AddAsync(Arg.Do<Domain.Entities.User>(u => addedUser = u)).Returns(Task.CompletedTask);
        _passwordService.HashPassword(request.Password).Returns("hashed_password");
        _tokenService.GenerateAccessToken(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(accessToken);
        _tokenService.GenerateRefreshToken(Arg.Any<Guid>()).Returns(refreshToken);
        _tokenService.StoreRefreshTokenAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>()).Returns(Task.CompletedTask);

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be(accessToken);
        result.RefreshToken.Should().Be(refreshToken);
        result.UserId.Should().Be(addedUser!.Id);
        result.Username.Should().Be(request.Username);
        result.Email.Should().Be(request.Email);
        result.Role.Should().Be("User");
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrowException_WhenEmailAlreadyExists()
    {
        // Arrange
        var request = new RegisterRequest(
            Email: "existing@example.com",
            Username: "testuser",
            Password: "password123"
        );

        _userRepository.ExistsByEmailAsync(request.Email).Returns(true);

        // Act
        Func<Task> act = async () => await _authService.RegisterAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Email is already in use.");
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrowException_WhenUsernameAlreadyExists()
    {
        // Arrange
        var request = new RegisterRequest(
            Email: "test@example.com",
            Username: "existinguser",
            Password: "password123"
        );

        _userRepository.ExistsByEmailAsync(request.Email).Returns(false);
        _userRepository.ExistsByUsernameAsync(request.Username).Returns(true);

        // Act
        Func<Task> act = async () => await _authService.RegisterAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Username is already in use.");
    }
}