namespace Identity.Test.Services.Token;

public class GenerateAccessTokenTests : TokenServiceBase
{
    [Fact]
    public void GenerateAccessToken_ShouldReturnTokenString()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "testuser";
        var email = "test@example.com";
        var role = "User";

        // Act
        var token = _tokenService.GenerateAccessToken(userId, username, email, role);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }
}
