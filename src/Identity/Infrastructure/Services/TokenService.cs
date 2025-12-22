using Application.Interfaces;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;
    private readonly string _jwtSecretKey;
    private readonly int _jwtExpiryMinutes;
    private readonly int _refreshTokenExpiryDays;
    private readonly IDatabase _database;
    public TokenService(IConnectionMultiplexer redis, IConfiguration configuration)
    {
        _database = redis.GetDatabase();
        _jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not found.");
        _jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience not found.");
        _jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT Key not found.");
        _jwtExpiryMinutes = int.Parse(Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES") ?? configuration["Jwt:AccessTokenExpirationMinutes"] ?? "15");
        _refreshTokenExpiryDays = int.Parse(Environment.GetEnvironmentVariable("REFRESH_TOKEN_EXPIRY_DAYS") ?? configuration["Jwt:RefreshTokenExpirationDays"] ?? "7");
    }
    public string GenerateAccessToken(Guid userId, string username, string email, string role, string jti = "")
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, jti == "" ? Guid.NewGuid().ToString() : jti)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _jwtIssuer,
            audience: _jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtExpiryMinutes),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken(Guid userId)
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    public bool ValidateAccessToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSecretKey);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtIssuer,
                ValidateAudience = true,
                ValidAudience = _jwtAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<Guid?> ValidateRefreshTokenAsync(
        string jti,
        string refreshToken)
    {
        var key = GetRefreshTokenKey(jti);
        var storedValue = await _database.StringGetAsync(key);

        if (storedValue.IsNullOrEmpty) return null;

        var parts = storedValue.ToString().Split(':');
        if (parts.Length != 2) return null;

        var storedUserId = parts[0];
        var storedHash = parts[1];

        if (Hash(refreshToken) != storedHash) return null;

        if (Guid.TryParse(storedUserId, out var userId))
            return userId;

        return null;
    }

    public async Task StoreRefreshTokenAsync(
        Guid userId,
        string jti,
        string refreshToken)
    {
        var key = GetRefreshTokenKey(jti);
        var value = $"{userId}:{Hash(refreshToken)}";

        await _database.StringSetAsync(
            key,
            value,
            TimeSpan.FromDays(_refreshTokenExpiryDays)
        );
    }

    public async Task<(Guid userId, string accessToken, string refreshToken)?> RefreshAsync(
        string oldJti,
        string refreshToken,
        string username,
        string email,
        string role)
    {
        var userId = await ValidateRefreshTokenAsync(oldJti, refreshToken);
        if (userId == null)
            return null;

        await _database.KeyDeleteAsync(GetRefreshTokenKey(oldJti));

        var newJti = Guid.NewGuid().ToString();
        var newAccessToken = GenerateAccessToken(userId.Value, username, email, role, newJti);
        var newRefreshToken = GenerateRefreshToken(userId.Value);

        await StoreRefreshTokenAsync(userId.Value, newJti, newRefreshToken);

        return (userId.Value, newAccessToken, newRefreshToken);
    }

    public async Task DeleteRefreshTokenAsync(string jti)
    {
        var key = GetRefreshTokenKey(jti);
        await _database.KeyDeleteAsync(key);
    }

    public async Task DeleteAllUserTokensAsync(Guid userId)
    {
        var pattern = $"rt:*";
        var endpoints = _database.Multiplexer.GetEndPoints();
        
        foreach (var endpoint in endpoints)
        {
            var server = _database.Multiplexer.GetServer(endpoint);
            var keys = server.Keys(pattern: pattern);
            
            foreach (var key in keys)
            {
                var value = await _database.StringGetAsync(key);
                if (!value.IsNullOrEmpty)
                {
                    var parts = value.ToString().Split(':');
                    if (parts.Length == 2 && parts[0] == userId.ToString())
                    {
                        await _database.KeyDeleteAsync(key);
                    }
                }
            }
        }
    }

    public static string Hash(string token) => Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    public string GetRefreshTokenKey(string jti) => $"rt:{jti}";
}
