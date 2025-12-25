namespace Identity.Test.Services.Token;

public class ValidateAccessTokenTests : TokenServiceBase
{
    [Fact]
    public void ValidateAccessToken_ShouldReturnTrue_WhenTokenIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = _tokenService.GenerateAccessToken(userId, "test", "test@test.com", "User");

        // Act
        var result = _tokenService.ValidateAccessToken(token);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateAccessToken_ShouldReturnFalse_WhenTokenIsInvalid()
    {
        // Arrange
        var token = "invalid_token";

        // Act
        var result = _tokenService.ValidateAccessToken(token);

        // Assert
        Assert.False(result);
    }
}
