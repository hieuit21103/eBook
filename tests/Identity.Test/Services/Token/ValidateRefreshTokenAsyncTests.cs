using Infrastructure.Services;
using StackExchange.Redis;

namespace Identity.Test.Services.Token;

public class ValidateRefreshTokenAsyncTests : TokenServiceBase
{
    [Fact]
    public async Task ValidateRefreshTokenAsync_ShouldReturnUserId_WhenTokenIsValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var jti = "test_jti";
        var refreshToken = "test_refresh_token";
        var hashedToken = TokenService.Hash(refreshToken);
        var storedValue = $"{userId}:{hashedToken}";

        _database.StringGetAsync(Arg.Is<RedisKey>(k => k == $"rt:{jti}"))
            .Returns((RedisValue)storedValue);

        // Act
        var result = await _tokenService.ValidateRefreshTokenAsync(jti, refreshToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result);
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_ShouldReturnNull_WhenTokenIsInvalid()
    {
        // Arrange
        var jti = "test_jti";
        var refreshToken = "test_refresh_token";
        var storedValue = $"{Guid.NewGuid()}:wrong_hash";

        _database.StringGetAsync(Arg.Is<RedisKey>(k => k == $"rt:{jti}"))
            .Returns((RedisValue)storedValue);

        // Act
        var result = await _tokenService.ValidateRefreshTokenAsync(jti, refreshToken);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_ShouldReturnNull_WhenTokenNotFound()
    {
        // Arrange
        var jti = "test_jti";
        var refreshToken = "test_refresh_token";

        _database.StringGetAsync(Arg.Is<RedisKey>(k => k == $"rt:{jti}"))
            .Returns(RedisValue.Null);

        // Act
        var result = await _tokenService.ValidateRefreshTokenAsync(jti, refreshToken);

        // Assert
        Assert.Null(result);
    }
}
