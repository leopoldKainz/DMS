using DMS.DAL.Entities;
using DMS.DAL.Enums;

namespace DMS.Business.Interfaces;

public interface IShareRequestService : IBaseService<ShareRequest>
{
    Task<IReadOnlyList<ShareRequest>> GetByOwnerAsync(Guid ownerId, CancellationToken ct = default);
    Task<IReadOnlyList<ShareRequest>> GetByRecieverAsync(Guid recieverId, CancellationToken ct = default);
    Task<IReadOnlyList<ShareRequest>> GetByDocumentAsync(Guid documentId, CancellationToken ct = default);
    Task<ShareRequest?> GetWithRelationsAsync(Guid shareRequestId, CancellationToken ct = default);
    Task<ShareRequest> CreateAsync(Guid ownerId, Guid recieverId, Guid documentId, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid ownerId, Guid recieverId, Guid documentId, CancellationToken ct = default);
    Task<bool> AcceptAsync(Guid shareRequestId, Permission permission, CancellationToken ct = default);
    Task<bool> DeclineAsync(Guid shareRequestId, CancellationToken ct = default);
}
