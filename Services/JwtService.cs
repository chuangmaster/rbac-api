using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using RbacApi.Services.Interfaces;

namespace RbacApi.Services;

public class JwtService : IJwtService
{
    private readonly RsaSecurityKey _signingKey;
    private readonly RsaSecurityKey _verificationKey;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtService(RSA rsa, IConfiguration configuration)
    {
        _signingKey = new RsaSecurityKey(rsa);

        var publicParams = rsa.ExportParameters(includePrivateParameters: false);
        var publicRsa = RSA.Create();
        publicRsa.ImportParameters(publicParams);
        _verificationKey = new RsaSecurityKey(publicRsa);

        _issuer = configuration["JWT:Issuer"] ?? "rbac-api";
        _audience = configuration["JWT:Audience"] ?? "mib_portal";
    }

    public string GenerateToken(string subject, string name, List<string> permissions)
    {
        var credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.RsaSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, subject),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Name, name),
        };

        // 每個 permission 作為獨立的 claim
        foreach (var perm in permissions)
        {
            claims.Add(new Claim("permissions", perm));
        }

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        RandomNumberGenerator.Fill(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public RsaSecurityKey GetPublicKey() => _verificationKey;

    public string GetPublicKeyJwkJson()
    {
        var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(_verificationKey);
        var publicJwk = new
        {
            kty = jwk.Kty,
            use = "sig",
            alg = "RS256",
            n = jwk.N,
            e = jwk.E,
        };
        return JsonSerializer.Serialize(publicJwk);
    }
}
