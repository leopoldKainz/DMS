using DMS.DAL.Entities;
using DMS.DAL.Enums;

namespace DMS.Business.Interfaces;

public interface IDocumentService : IBaseService<Document>
{
    Task<IReadOnlyList<Document>> GetByUserAsync(int userId, CancellationToken ct = default);
    Task<IReadOnlyList<Document>> GetByUserAsync(int userId, Permission permission, CancellationToken ct = default);
    Task<IReadOnlyList<Document>> SearchByNameAsync(int userId, string searchTerm, CancellationToken ct = default);
    Task<Document?> GetWithUsersAsync(int documentId, CancellationToken ct = default);
    Task<Document> AddForOwnerAsync(int ownerUserId, Document document, CancellationToken ct = default);
    Task<bool> HasAccessAsync(int userId, int documentId, CancellationToken ct = default);
    Task<bool> HasPermissionAsync(int userId, int documentId, Permission permission, CancellationToken ct = default);
}
