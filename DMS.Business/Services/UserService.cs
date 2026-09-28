using System.Security.Cryptography;
using System.Text;
using DMS.Business.Interfaces;
using DMS.DAL;
using DMS.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DMS.Business.Services;

public class UserService : BaseService<User>, IUserService
{
    public UserService(DmsDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Username == username, ct);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default)
    {
        return await _dbSet.AnyAsync(u => u.Username == username, ct);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
    {
        return await _dbSet.AnyAsync(u => u.Email == email, ct);
    }

    public async Task<User?> GetWithDocumentsAsync(int userId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(u => u.UserDocuments)
            .ThenInclude(ud => ud.Document)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);
    }

    public async Task<User?> AuthenticateAsync(string usernameOrEmail, string password, CancellationToken ct = default)
    {
        var user = await _dbSet.FirstOrDefaultAsync(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail, ct);
        if (user == null)
        {
            return null;
        }

        if (VerifyPassword(password, user.Password))
        {
            return user;
        }

        return null;
    }

    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword, CancellationToken ct = default)
    {
        var user = await _dbSet.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user == null)
        {
            return false;
        }

        if (!VerifyPassword(currentPassword, user.Password))
        {
            return false;
        }

        user.Password = HashPassword(newPassword);
        _dbSet.Update(user);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public override async Task<User> AddAsync(User entity, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(entity.Password))
        {
            throw new ArgumentException("Password is required.");
        }

        entity.Password = HashPassword(entity.Password);
        return await base.AddAsync(entity, ct);
    }

    public static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }

    public static bool VerifyPassword(string password, string storedHash)
    {
        var hash = HashPassword(password);
        return string.Equals(hash, storedHash, StringComparison.OrdinalIgnoreCase);
    }
}
