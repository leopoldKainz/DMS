using DMS.Business.Interfaces;
using DMS.DAL.Entities;
using DMS.DAL.Enums;

namespace DMS.Business.Services;

public class DocumentService : IDocumentService
{
    //TODO: Implement the methods of the IDocumentService interface using data access layer.
    public Task<Document?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<Document?> GetByEntityIdAsync(Guid entityId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<Document>> GetAllAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExistsAsync(int id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<Document> AddAsync(Document entity, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public void Update(Document entity)
    {
        throw new NotImplementedException();
    }

    public void Remove(Document entity)
    {
        throw new NotImplementedException();
    }

    public Task<bool> RemoveByIdAsync(int id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<Document>> GetByUserAsync(int userId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<Document>> GetByUserAsync(int userId, Permission permission, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<Document>> SearchByNameAsync(int userId, string searchTerm, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<Document?> GetWithUsersAsync(int documentId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<Document> AddForOwnerAsync(int ownerUserId, Document document, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> HasAccessAsync(int userId, int documentId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> HasPermissionAsync(int userId, int documentId, Permission permission, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}