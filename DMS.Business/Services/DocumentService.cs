using DMS.Business.Interfaces;
using DMS.Business.Models.Validation;
using DMS.DAL;
using DMS.DAL.Entities;
using DMS.DAL.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DMS.Business.Services;

public class DocumentService : BaseService<Document>, IDocumentService
{
    private readonly IValidator<Document> _validator;

    public DocumentService(DmsDbContext context, ILogger<DocumentService> logger) : this(context, new DocumentValidator(), logger)
    {
    }

    public DocumentService(DmsDbContext context, IValidator<Document> validator, ILogger<DocumentService> logger) : base(context, logger)
    {
        _validator = validator;
    }

    public override async Task<Document> AddAsync(Document entity, CancellationToken ct = default)
    {
        var validationResult = await _validator.ValidateAsync(entity, ct);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        return await base.AddAsync(entity, ct);
    }

    public async Task<IReadOnlyList<Document>> GetByUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.UserDocuments
            .Where(ud => ud.User.EntityId == userId)
            .Select(ud => ud.Document)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Document>> GetByUserAsync(Guid userId, Permission permission, CancellationToken ct = default)
    {
        return await _context.UserDocuments
            .Where(ud => ud.User.EntityId == userId && ud.Permission == permission)
            .Select(ud => ud.Document)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Document>> SearchByNameAsync(Guid userId, string searchTerm, CancellationToken ct = default)
    {
        return await _context.UserDocuments
            .Where(ud => ud.User.EntityId == userId && EF.Functions.ILike(ud.Document.Name, $"%{searchTerm}%"))
            .Select(ud => ud.Document)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<Document?> GetWithUsersAsync(Guid documentId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(d => d.UserDocuments)
            .ThenInclude(ud => ud.User)
            .FirstOrDefaultAsync(d => d.EntityId == documentId, ct);
    }

    public async Task<Document> AddForOwnerAsync(Guid ownerUserId, Document document, CancellationToken ct = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.EntityId == ownerUserId, ct);
        if (user == null)
        {
            throw new ArgumentException($"User with ID {ownerUserId} not found.", nameof(ownerUserId));
        }

        var validationResult = await _validator.ValidateAsync(document, ct);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var userDocument = new UserDocument
        {
            UserId = user.Id,
            User = user,
            Document = document,
            Permission = Permission.Owner
        };

        await _dbSet.AddAsync(document, ct);
        await _context.UserDocuments.AddAsync(userDocument, ct);

        _logger.LogInformation("Document {DocumentId} ({DocumentName}) uploaded by user {UserId}",
            document.EntityId, document.Name, ownerUserId);
        return document;
    }

    public async Task<bool> HasAccessAsync(Guid userId, Guid documentId, CancellationToken ct = default)
    {
        return await _context.UserDocuments
            .AnyAsync(ud => ud.User.EntityId == userId && ud.Document.EntityId == documentId, ct);
    }

    public async Task<bool> HasPermissionAsync(Guid userId, Guid documentId, Permission permission, CancellationToken ct = default)
    {
        return await _context.UserDocuments
            .AnyAsync(ud => ud.User.EntityId == userId && ud.Document.EntityId == documentId && ud.Permission == permission, ct);
    }
}
