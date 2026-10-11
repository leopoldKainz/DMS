using DMS.DAL.Entities;
using DMS.DAL.Enums;

namespace DMS.Business.Interfaces;

// UserDocument is a join entity with the composite key (UserId, DocumentId)
// and no BaseEntity.Id, so it cannot use IBaseService<T>.
public interface IUserDocumentService
{
    Task<UserDocument?> GetAsync(Guid userId, Guid documentId, CancellationToken ct = default);
    Task<IReadOnlyList<UserDocument>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<UserDocument>> GetByDocumentAsync(Guid documentId, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid userId, Guid documentId, CancellationToken ct = default);
    Task<bool> HasPermissionAsync(Guid userId, Guid documentId, Permission permission, CancellationToken ct = default);
    Task<UserDocument> GrantAsync(Guid userId, Guid documentId, Permission permission, CancellationToken ct = default);
    Task<bool> UpdatePermissionAsync(Guid userId, Guid documentId, Permission permission, CancellationToken ct = default);
    Task<bool> RevokeAsync(Guid userId, Guid documentId, CancellationToken ct = default);
}
