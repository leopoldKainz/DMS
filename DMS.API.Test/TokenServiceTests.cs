using DMS.API.Auth;
using DMS.DAL.Entities;
using Microsoft.Extensions.Options;

namespace DMS.API.Test;

public class TokenServiceTests
{
    [Test]
    public void CreateToken_ValidKey_GeneratesToken()
    {
        var options = Options.Create(new JwtOptions
        {
            Issuer = "DMS.API",
            Audience = "DMS.Web",
            Key = "dms_development_secret_key_must_be_at_least_32_bytes_long!",
            ExpiresMinutes = 60
        });

        var service = new TokenService(options);
        var user = new User { EntityId = Guid.NewGuid(), Username = "testuser", Email = "test@example.com" };

        var (token, expiresAt) = service.CreateToken(user);

        Assert.That(token, Is.Not.Null.And.Not.Empty);
        Assert.That(expiresAt, Is.GreaterThan(DateTime.UtcNow));
    }

    [Test]
    public void CreateToken_KeyTooShort_ThrowsArgumentException()
    {
        var options = Options.Create(new JwtOptions
        {
            Issuer = "DMS.API",
            Audience = "DMS.Web",
            Key = "temporary_jwt_key", // 17 bytes (136 bits) -> < 256 bits
            ExpiresMinutes = 60
        });

        var service = new TokenService(options);
        var user = new User { EntityId = Guid.NewGuid(), Username = "testuser", Email = "test@example.com" };

        Assert.Throws<ArgumentOutOfRangeException>(() => service.CreateToken(user));
    }
}
