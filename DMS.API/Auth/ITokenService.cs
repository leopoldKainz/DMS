using DMS.DAL.Entities;

namespace DMS.API.Auth;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(User user);
}
