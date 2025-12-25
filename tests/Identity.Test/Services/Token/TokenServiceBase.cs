using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace Identity.Test.Services.Token;

public class TokenServiceBase
{
    protected readonly IConnectionMultiplexer _redis;
    protected readonly IDatabase _database;
    protected readonly IConfiguration _configuration;
    protected readonly TokenService _tokenService;

    public TokenServiceBase()
    {
        _redis = Substitute.For<IConnectionMultiplexer>();
        _database = Substitute.For<IDatabase>();
        _configuration = Substitute.For<IConfiguration>();

        _redis.GetDatabase(Arg.Any<int>(), Arg.Any<object>()).Returns(_database);
        _database.Multiplexer.Returns(_redis);

        // Setup configuration
        _configuration["Jwt:Issuer"].Returns("test_issuer");
        _configuration["Jwt:Audience"].Returns("test_audience");
        _configuration["Jwt:SecretKey"].Returns("super_secret_key_for_testing_purposes_only_12345");
        _configuration["Jwt:AccessTokenExpirationMinutes"].Returns("15");
        _configuration["Jwt:RefreshTokenExpirationDays"].Returns("7");

        _tokenService = new TokenService(_redis, _configuration);
    }
}
