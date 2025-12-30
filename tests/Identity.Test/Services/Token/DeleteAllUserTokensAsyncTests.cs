using StackExchange.Redis;
using System.Net;

namespace Identity.Test.Services.Token;

public class DeleteAllUserTokensAsyncTests : TokenServiceBase
{
    [Fact]
    public async Task DeleteAllUserTokensAsync_ShouldDeleteAllTokensForUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var endpoint = new DnsEndPoint("localhost", 6379);
        var server = Substitute.For<IServer>();

        _redis.GetEndPoints().Returns(new EndPoint[] { endpoint });
        _redis.GetServer(endpoint).Returns(server);

        var keys = new RedisKey[] { "rt:jti1", "rt:jti2", "rt:jti3" };
        server.Keys(pattern: "rt:*").Returns(keys);

        // Mock values for keys
        // Key 1: Matches user
        _database.StringGetAsync("rt:jti1").Returns((RedisValue)$"{userId}:hash1");
        // Key 2: Matches different user
        _database.StringGetAsync("rt:jti2").Returns((RedisValue)$"{Guid.NewGuid()}:hash2");
        // Key 3: Invalid format
        _database.StringGetAsync("rt:jti3").Returns((RedisValue)"invalid_format");

        // Act
        await _tokenService.DeleteAllUserTokensAsync(userId);

        // Assert
        // Should delete key 1
        await _database.Received(1).KeyDeleteAsync("rt:jti1");

        // Should NOT delete key 2 or 3
        await _database.DidNotReceive().KeyDeleteAsync("rt:jti2");
        await _database.DidNotReceive().KeyDeleteAsync("rt:jti3");
    }
}
