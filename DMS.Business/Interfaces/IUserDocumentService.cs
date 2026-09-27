using DMS.DAL.Entities;
using DMS.DAL.Enums;

namespace DMS.Business.Interfaces;

// UserDocument is a join entity with the composite key (UserId, DocumentId)
// and no BaseEntity.Id, so it cannot use IBaseService<T>.
public interface IUserDocumentService
{
    Task<UserDocument?> GetAsync(int userId, int documentId, CancellationToken ct = default);
    Task<IReadOnlyList<UserDocument>> GetByUserAsync(int userId, CancellationToken ct = default);
    Task<IReadOnlyList<UserDocument>> GetByDocumentAsync(int documentId, CancellationToken ct = default);
    Task<bool> ExistsAsync(int userId, int documentId, CancellationToken ct = default);
    Task<bool> HasPermissionAsync(int userId, int documentId, Permission permission, CancellationToken ct = default);
    Task<UserDocument> GrantAsync(int userId, int documentId, Permission permission, CancellationToken ct = default);
    Task<bool> UpdatePermissionAsync(int userId, int documentId, Permission permission, CancellationToken ct = default);
    Task<bool> RevokeAsync(int userId, int documentId, CancellationToken ct = default);
}
