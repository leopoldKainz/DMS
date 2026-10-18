using System.Security.Cryptography;
using System.Text;
using DMS.Business.Interfaces;
using DMS.Business.Models.Validation;
using DMS.DAL;
using DMS.DAL.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DMS.Business.Services;

public class UserService : BaseService<User>, IUserService
{
    private readonly IValidator<User> _validator;

    public UserService(DmsDbContext context, ILogger<UserService> logger) : this(context, new UserValidator(), logger)
    {
    }

    public UserService(DmsDbContext context, IValidator<User> validator, ILogger<UserService> logger) : base(context, logger)
    {
        _validator = validator;
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

    public async Task<User?> GetWithDocumentsAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(u => u.UserDocuments)
            .ThenInclude(ud => ud.Document)
            .FirstOrDefaultAsync(u => u.EntityId == userId, ct);
    }

    public async Task<User?> AuthenticateAsync(string usernameOrEmail, string password, CancellationToken ct = default)
    {
        var user = await _dbSet.FirstOrDefaultAsync(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail, ct);
        if (user == null)
        {
            _logger.LogWarning("Login failed: unknown user");
            return null;
        }

        if (VerifyPassword(password, user.Password, out bool needsRehash))
        {
            if (needsRehash)
            {
                user.Password = HashPassword(password);
                _logger.LogInformation("Password hash upgraded for user {UserId}", user.EntityId);
            }

            _logger.LogInformation("User {UserId} authenticated", user.EntityId);
            return user;
        }

        _logger.LogWarning("Login failed: wrong password for user {UserId}", user.EntityId);
        return null;
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken ct = default)
    {
        var user = await _dbSet.FirstOrDefaultAsync(u => u.EntityId == userId, ct);
        if (user == null)
        {
            return false;
        }

        if (!VerifyPassword(currentPassword, user.Password))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(newPassword))
        {
            throw new ArgumentException("New password is required.", nameof(newPassword));
        }

        user.Password = HashPassword(newPassword);
        _dbSet.Update(user);

        _logger.LogInformation("Password changed for user {UserId}", userId);
        return true;
    }

    public override async Task<User> AddAsync(User entity, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(entity.Password))
        {
            throw new ArgumentException("Password is required.", nameof(entity));
        }

        var validationResult = await _validator.ValidateAsync(entity, ct);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        entity.Password = HashPassword(entity.Password);
        return await base.AddAsync(entity, ct);
    }

    private const string HashVersion = "v1";
    private const int HashIterations = 600_000;
    private const int SaltSize = 16;
    private const int KeySize = 32;

    public static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, HashIterations, HashAlgorithmName.SHA256, KeySize);
        return $"{HashVersion}.{HashIterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }

    public static bool VerifyPassword(string password, string storedHash)
    {
        return VerifyPassword(password, storedHash, out _);
    }

    private static bool VerifyPassword(string password, string storedHash, out bool needsRehash)
    {
        needsRehash = false;
        var parts = storedHash.Split('.');

        if (parts.Length == 1)
        {
            needsRehash = true;
            var legacy = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return CryptographicOperations.FixedTimeEquals(
                Encoding.ASCII.GetBytes(Convert.ToHexString(legacy)),
                Encoding.ASCII.GetBytes(storedHash.ToUpperInvariant()));
        }

        if (parts.Length != 4 || parts[0] != HashVersion || !int.TryParse(parts[1], out int iterations))
        {
            return false;
        }

        var salt = Convert.FromBase64String(parts[2]);
        var expected = Convert.FromBase64String(parts[3]);
        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);

        needsRehash = iterations != HashIterations;
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
