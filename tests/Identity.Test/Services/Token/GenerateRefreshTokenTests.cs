namespace Identity.Test.Services.Token;

public class GenerateRefreshTokenTests : TokenServiceBase
{
    [Fact]
    public void GenerateRefreshToken_ShouldReturnString()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var token = _tokenService.GenerateRefreshToken(userId);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }
}
