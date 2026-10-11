using DMS.DAL.Entities;

namespace DMS.Business.Interfaces;

public interface IUserService : IBaseService<User>
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    Task<User?> GetWithDocumentsAsync(Guid userId, CancellationToken ct = default);
    Task<User?> AuthenticateAsync(string usernameOrEmail, string password, CancellationToken ct = default);
    Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken ct = default);
}
