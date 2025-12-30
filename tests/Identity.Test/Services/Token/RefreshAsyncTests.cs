using StackExchange.Redis;

namespace Identity.Test.Services.Token;

public class RefreshAsyncTests : TokenServiceBase
{
    [Fact]
    public async Task RefreshAsync_ShouldReturnNewTokens_WhenRefreshTokenIsValid()
    {
        // Arrange
        var oldJti = "old_jti";
        var refreshToken = "valid_refresh_token";
        var userId = Guid.NewGuid();
        var username = "testuser";
        var email = "test@example.com";
        var role = "User";

        var hashedToken = Infrastructure.Services.TokenService.Hash(refreshToken);
        var storedValue = $"{userId}:{hashedToken}";

        _database.StringGetAsync($"rt:{oldJti}").Returns((RedisValue)storedValue);

        // Act
        var result = await _tokenService.RefreshAsync(oldJti, refreshToken, username, email, role);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Value.userId);
        Assert.NotNull(result.Value.accessToken);
        Assert.NotNull(result.Value.refreshToken);

        // Verify old token deleted
        await _database.Received(1).KeyDeleteAsync($"rt:{oldJti}");

        // Verify new token stored
        await _database.Received(1).StringSetAsync(
            Arg.Is<RedisKey>(k => k.ToString().StartsWith("rt:")),
            Arg.Any<RedisValue>(),
            Arg.Any<Expiration>()
        );
    }

    [Fact]
    public async Task RefreshAsync_ShouldReturnNull_WhenRefreshTokenIsInvalid()
    {
        // Arrange
        var oldJti = "old_jti";
        var refreshToken = "invalid_token";

        _database.StringGetAsync($"rt:{oldJti}").Returns(RedisValue.Null);

        // Act
        var result = await _tokenService.RefreshAsync(oldJti, refreshToken, "user", "email", "role");

        // Assert
        Assert.Null(result);
    }
}
