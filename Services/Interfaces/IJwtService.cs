using Microsoft.IdentityModel.Tokens;

namespace RbacApi.Services.Interfaces;

public interface IJwtService
{
    string GenerateToken(string subject, string name, List<string> permissions);
    string GenerateRefreshToken();
    RsaSecurityKey GetPublicKey();
    string GetPublicKeyJwkJson();
}
