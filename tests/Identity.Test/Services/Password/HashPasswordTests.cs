namespace Identity.Test.Services.Password;

public class HashPasswordTests : PasswordServiceBase
{
    [Fact]
    public void HashPassword_ShouldReturnHashString()
    {
        // Arrange
        var password = "password123";

        // Act
        var hash = _passwordService.HashPassword(password);

        // Assert
        Assert.NotNull(hash);
        Assert.NotEmpty(hash);
        Assert.NotEqual(password, hash);
    }
}
