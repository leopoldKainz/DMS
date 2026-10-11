using DMS.DAL.Entities;
using DMS.DAL.Enums;

namespace DMS.Business.Interfaces;

public interface IDocumentService : IBaseService<Document>
{
    Task<IReadOnlyList<Document>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<Document>> GetByUserAsync(Guid userId, Permission permission, CancellationToken ct = default);
    Task<IReadOnlyList<Document>> SearchByNameAsync(Guid userId, string searchTerm, CancellationToken ct = default);
    Task<Document?> GetWithUsersAsync(Guid documentId, CancellationToken ct = default);
    Task<Document> AddForOwnerAsync(Guid ownerUserId, Document document, CancellationToken ct = default);
    Task<bool> HasAccessAsync(Guid userId, Guid documentId, CancellationToken ct = default);
    Task<bool> HasPermissionAsync(Guid userId, Guid documentId, Permission permission, CancellationToken ct = default);
}
