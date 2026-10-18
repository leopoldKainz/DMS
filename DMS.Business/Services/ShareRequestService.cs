using DMS.Business.Interfaces;
using DMS.DAL;
using DMS.DAL.Entities;
using DMS.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DMS.Business.Services;

public class ShareRequestService : BaseService<ShareRequest>, IShareRequestService
{
    public ShareRequestService(DmsDbContext context, ILogger<ShareRequestService> logger) : base(context, logger)
    {
    }

    public async Task<IReadOnlyList<ShareRequest>> GetByOwnerAsync(Guid ownerId, CancellationToken ct = default)
    {
        return await WithRelations()
            .Where(sr => sr.Owner.EntityId == ownerId)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ShareRequest>> GetByRecieverAsync(Guid recieverId, CancellationToken ct = default)
    {
        return await WithRelations()
            .Where(sr => sr.Reciever.EntityId == recieverId)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ShareRequest>> GetByDocumentAsync(Guid documentId, CancellationToken ct = default)
    {
        return await WithRelations()
            .Where(sr => sr.Document.EntityId == documentId)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<ShareRequest?> GetWithRelationsAsync(Guid shareRequestId, CancellationToken ct = default)
    {
        return await WithRelations().FirstOrDefaultAsync(sr => sr.EntityId == shareRequestId, ct);
    }

    public async Task<ShareRequest> CreateAsync(Guid ownerId, Guid recieverId, Guid documentId, CancellationToken ct = default)
    {
        if (ownerId == recieverId)
        {
            throw new ArgumentException("A document cannot be shared with its owner.");
        }

        var owner = await _context.Users.FirstOrDefaultAsync(u => u.EntityId == ownerId, ct)
            ?? throw new ArgumentException($"User with ID {ownerId} not found.", nameof(ownerId));
        var reciever = await _context.Users.FirstOrDefaultAsync(u => u.EntityId == recieverId, ct)
            ?? throw new ArgumentException($"User with ID {recieverId} not found.", nameof(recieverId));
        var document = await _context.Documents.FirstOrDefaultAsync(d => d.EntityId == documentId, ct)
            ?? throw new ArgumentException($"Document with ID {documentId} not found.", nameof(documentId));

        var isOwner = await _context.UserDocuments
            .AnyAsync(ud => ud.UserId == owner.Id && ud.DocumentId == document.Id && ud.Permission == Permission.Owner, ct);
        if (!isOwner)
        {
            _logger.LogWarning("User {UserId} tried to share document {DocumentId} without being its owner", ownerId, documentId);
            throw new UnauthorizedAccessException($"User {ownerId} is not the owner of document {documentId}.");
        }

        if (await _context.UserDocuments.AnyAsync(ud => ud.UserId == reciever.Id && ud.DocumentId == document.Id, ct))
        {
            throw new ArgumentException($"User {recieverId} already has access to document {documentId}.");
        }

        if (await ExistsAsync(ownerId, recieverId, documentId, ct))
        {
            throw new ArgumentException("This share request already exists.");
        }

        var shareRequest = new ShareRequest
        {
            Owner = owner,
            Reciever = reciever,
            Document = document
        };

        await _dbSet.AddAsync(shareRequest, ct);

        _logger.LogInformation("Share request {ShareRequestId} created: document {DocumentId} from {OwnerId} to {RecieverId}",
            shareRequest.EntityId, documentId, ownerId, recieverId);
        return shareRequest;
    }

    public async Task<bool> ExistsAsync(Guid ownerId, Guid recieverId, Guid documentId, CancellationToken ct = default)
    {
        return await _dbSet.AnyAsync(sr =>
            sr.Owner.EntityId == ownerId &&
            sr.Reciever.EntityId == recieverId &&
            sr.Document.EntityId == documentId, ct);
    }

    public async Task<bool> AcceptAsync(Guid shareRequestId, Permission permission, CancellationToken ct = default)
    {
        var shareRequest = await GetWithRelationsAsync(shareRequestId, ct);
        if (shareRequest == null)
        {
            return false;
        }

        var reciever = shareRequest.Reciever;
        var document = shareRequest.Document;

        var hasAccess = await _context.UserDocuments
            .AnyAsync(ud => ud.UserId == reciever.Id && ud.DocumentId == document.Id, ct);
        if (!hasAccess)
        {
            await _context.UserDocuments.AddAsync(new UserDocument
            {
                UserId = reciever.Id,
                User = reciever,
                DocumentId = document.Id,
                Document = document,
                Permission = permission
            }, ct);
        }

        _dbSet.Remove(shareRequest);

        _logger.LogInformation("Share request {ShareRequestId} accepted: user {UserId} got {Permission} on document {DocumentId}",
            shareRequestId, reciever.EntityId, permission, document.EntityId);
        return true;
    }

    public async Task<bool> DeclineAsync(Guid shareRequestId, CancellationToken ct = default)
    {
        return await RemoveByIdAsync(shareRequestId, ct);
    }

    private IQueryable<ShareRequest> WithRelations()
    {
        return _dbSet
            .Include(sr => sr.Owner)
            .Include(sr => sr.Reciever)
            .Include(sr => sr.Document);
    }
}
