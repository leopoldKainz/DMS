using DMS.Business.Interfaces;
using DMS.DAL.Entities;

namespace DMS.Business
{
    public class UserService : IUserService
    {
        //TODO: Implement the methods of IUserService interface using the data access layer.
        public Task<User?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<User?> GetByEntityIdAsync(Guid entityId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<User> AddAsync(User entity, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public void Update(User entity)
        {
            throw new NotImplementedException();
        }

        public void Remove(User entity)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemoveByIdAsync(int id, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<User?> GetWithDocumentsAsync(int userId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<User?> AuthenticateAsync(string usernameOrEmail, string password, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
