using System.Security.Claims;
using System.Text;
using DMS.DAL.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace DMS.API.Auth;

public class TokenService : ITokenService
{
    private readonly JwtOptions _options;

    public TokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public (string Token, DateTime ExpiresAt) CreateToken(User user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_options.ExpiresMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.EntityId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        if (user.Username != null)
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.UniqueName, user.Username));
        }
        if (user.Email != null)
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
        }

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Expires = expiresAt,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        };

        string token = new JsonWebTokenHandler().CreateToken(descriptor);
        return (token, expiresAt);
    }
}
