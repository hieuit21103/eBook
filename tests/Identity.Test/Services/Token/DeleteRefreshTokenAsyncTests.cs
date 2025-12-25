using StackExchange.Redis;

namespace Identity.Test.Services.Token;

public class DeleteRefreshTokenAsyncTests : TokenServiceBase
{
    [Fact]
    public async Task DeleteRefreshTokenAsync_ShouldDeleteKeyFromRedis()
    {
        // Arrange
        var userId = "user123";
        var jti = "test_jti";

        // Act
        await _tokenService.DeleteRefreshTokenAsync(userId, jti);

        // Assert
        await _database.Received(1).KeyDeleteAsync($"rt:{jti}");
        await _database.Received(1).SetRemoveAsync($"user_rt:{userId}", jti);
    }
}
