using DMS.Business.Interfaces;
using DMS.DAL;
using DMS.DAL.Entities;
using DMS.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace DMS.Business.Services;

public class DocumentService : BaseService<Document>, IDocumentService
{
    public DocumentService(DmsDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Document>> GetByUserAsync(int userId, CancellationToken ct = default)
    {
        return await _context.UserDocuments
            .Where(ud => ud.UserId == userId)
            .Select(ud => ud.Document)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Document>> GetByUserAsync(int userId, Permission permission, CancellationToken ct = default)
    {
        return await _context.UserDocuments
            .Where(ud => ud.UserId == userId && ud.Permission == permission)
            .Select(ud => ud.Document)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Document>> SearchByNameAsync(int userId, string searchTerm, CancellationToken ct = default)
    {
        return await _context.UserDocuments
            .Where(ud => ud.UserId == userId && EF.Functions.ILike(ud.Document.Name, $"%{searchTerm}%"))
            .Select(ud => ud.Document)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<Document?> GetWithUsersAsync(int documentId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(d => d.UserDocuments)
            .ThenInclude(ud => ud.User)
            .FirstOrDefaultAsync(d => d.Id == documentId, ct);
    }

    public async Task<Document> AddForOwnerAsync(int ownerUserId, Document document, CancellationToken ct = default)
    {
        var userExists = await _context.Users.AnyAsync(u => u.Id == ownerUserId, ct);
        if (!userExists)
        {
            throw new ArgumentException($"User with ID {ownerUserId} not found.", nameof(ownerUserId));
        }

        await _dbSet.AddAsync(document, ct);
        await _context.SaveChangesAsync(ct);

        var userDocument = new UserDocument
        {
            UserId = ownerUserId,
            DocumentId = document.Id,
            Permission = Permission.Owner
        };

        await _context.UserDocuments.AddAsync(userDocument, ct);
        await _context.SaveChangesAsync(ct);

        return document;
    }

    public async Task<bool> HasAccessAsync(int userId, int documentId, CancellationToken ct = default)
    {
        return await _context.UserDocuments
            .AnyAsync(ud => ud.UserId == userId && ud.DocumentId == documentId, ct);
    }

    public async Task<bool> HasPermissionAsync(int userId, int documentId, Permission permission, CancellationToken ct = default)
    {
        return await _context.UserDocuments
            .AnyAsync(ud => ud.UserId == userId && ud.DocumentId == documentId && ud.Permission == permission, ct);
    }
}
