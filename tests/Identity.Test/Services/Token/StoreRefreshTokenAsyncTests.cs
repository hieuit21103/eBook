using Infrastructure.Services;
using StackExchange.Redis;

namespace Identity.Test.Services.Token;

public class StoreRefreshTokenAsyncTests : TokenServiceBase
{
    [Fact]
    public async Task StoreRefreshTokenAsync_ShouldStoreTokenInRedis()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var jti = "test_jti";
        var refreshToken = "test_refresh_token";

        // Act
        await _tokenService.StoreRefreshTokenAsync(userId, jti, refreshToken);

        // Assert
        await _database.Received(1).StringSetAsync(
            Arg.Is<RedisKey>(k => k == $"rt:{jti}"),
            Arg.Any<RedisValue>(),
            Arg.Any<Expiration>()
        );
    }
}
