using System.Security.Cryptography;
using RbacApi.Services.Interfaces;
using StackExchange.Redis;

namespace RbacApi.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IConnectionMultiplexer _redis;
    private static readonly TimeSpan TokenExpiry = TimeSpan.FromDays(7);
    private const string KeyPrefix = "refresh_token:";

    public RefreshTokenService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<string> CreateAsync(string employeeId)
    {
        var token = GenerateToken();
        var db = _redis.GetDatabase();
        await db.StringSetAsync($"{KeyPrefix}{token}", employeeId, TokenExpiry);
        return token;
    }

    public async Task<string?> ValidateAsync(string refreshToken)
    {
        var db = _redis.GetDatabase();
        var employeeId = await db.StringGetAsync($"{KeyPrefix}{refreshToken}");
        return employeeId.IsNullOrEmpty ? null : employeeId.ToString();
    }

    public async Task RevokeAsync(string refreshToken)
    {
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync($"{KeyPrefix}{refreshToken}");
    }

    private static string GenerateToken()
    {
        var randomBytes = new byte[64];
        RandomNumberGenerator.Fill(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
