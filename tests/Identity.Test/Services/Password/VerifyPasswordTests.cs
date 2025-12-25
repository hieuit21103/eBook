namespace Identity.Test.Services.Password;

public class VerifyPasswordTests : PasswordServiceBase
{
    [Fact]
    public void VerifyPassword_ShouldReturnTrue_WhenPasswordMatchesHash()
    {
        // Arrange
        var password = "password123";
        var hash = _passwordService.HashPassword(password);

        // Act
        var result = _passwordService.VerifyPassword(password, hash);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_WhenPasswordDoesNotMatchHash()
    {
        // Arrange
        var password = "password123";
        var hash = _passwordService.HashPassword(password);
        var wrongPassword = "wrongpassword";

        // Act
        var result = _passwordService.VerifyPassword(wrongPassword, hash);

        // Assert
        Assert.False(result);
    }
}
