using DMS.Business.Interfaces;
using DMS.DAL;
using DMS.DAL.Entities;
using DMS.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DMS.Business.Services;

public class UserDocumentService : IUserDocumentService
{
    private readonly DmsDbContext _context;
    private readonly ILogger<UserDocumentService> _logger;

    public UserDocumentService(DmsDbContext context, ILogger<UserDocumentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UserDocument?> GetAsync(Guid userId, Guid documentId, CancellationToken ct = default)
    {
        return await _context.UserDocuments
            .FirstOrDefaultAsync(ud => ud.User.EntityId == userId && ud.Document.EntityId == documentId, ct);
    }

    public async Task<IReadOnlyList<UserDocument>> GetByUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.UserDocuments
            .Include(ud => ud.Document)
            .Where(ud => ud.User.EntityId == userId)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<UserDocument>> GetByDocumentAsync(Guid documentId, CancellationToken ct = default)
    {
        return await _context.UserDocuments
            .Include(ud => ud.User)
            .Where(ud => ud.Document.EntityId == documentId)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsAsync(Guid userId, Guid documentId, CancellationToken ct = default)
    {
        return await _context.UserDocuments
            .AnyAsync(ud => ud.User.EntityId == userId && ud.Document.EntityId == documentId, ct);
    }

    public async Task<bool> HasPermissionAsync(Guid userId, Guid documentId, Permission permission, CancellationToken ct = default)
    {
        return await _context.UserDocuments
            .AnyAsync(ud => ud.User.EntityId == userId && ud.Document.EntityId == documentId && ud.Permission == permission, ct);
    }

    public async Task<UserDocument> GrantAsync(Guid userId, Guid documentId, Permission permission, CancellationToken ct = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.EntityId == userId, ct)
            ?? throw new ArgumentException($"User with ID {userId} not found.", nameof(userId));
        var document = await _context.Documents.FirstOrDefaultAsync(d => d.EntityId == documentId, ct)
            ?? throw new ArgumentException($"Document with ID {documentId} not found.", nameof(documentId));

        if (await _context.UserDocuments.AnyAsync(ud => ud.UserId == user.Id && ud.DocumentId == document.Id, ct))
        {
            throw new ArgumentException($"User {userId} already has access to document {documentId}.");
        }

        var userDocument = new UserDocument
        {
            UserId = user.Id,
            User = user,
            DocumentId = document.Id,
            Document = document,
            Permission = permission
        };

        await _context.UserDocuments.AddAsync(userDocument, ct);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Granted {Permission} on document {DocumentId} to user {UserId}", permission, documentId, userId);
        return userDocument;
    }

    public async Task<bool> UpdatePermissionAsync(Guid userId, Guid documentId, Permission permission, CancellationToken ct = default)
    {
        var userDocument = await GetAsync(userId, documentId, ct);
        if (userDocument == null)
        {
            return false;
        }

        userDocument.Permission = permission;
        _context.UserDocuments.Update(userDocument);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Changed permission on document {DocumentId} for user {UserId} to {Permission}", documentId, userId, permission);
        return true;
    }

    public async Task<bool> RevokeAsync(Guid userId, Guid documentId, CancellationToken ct = default)
    {
        var userDocument = await GetAsync(userId, documentId, ct);
        if (userDocument == null)
        {
            return false;
        }

        _context.UserDocuments.Remove(userDocument);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Revoked access on document {DocumentId} for user {UserId}", documentId, userId);
        return true;
    }
}
