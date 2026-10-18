using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace DMS.API.Auth;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        string? sub = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (!Guid.TryParse(sub, out Guid userId))
        {
            throw new UnauthorizedAccessException("Authenticated user has no valid 'sub' claim.");
        }

        return userId;
    }
}
