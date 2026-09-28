using DMS.DAL.Entities;
using DMS.DAL.Enums;

namespace DMS.Business.Interfaces;

public interface IShareRequestService : IBaseService<ShareRequest>
{
    Task<IReadOnlyList<ShareRequest>> GetByOwnerAsync(int ownerId, CancellationToken ct = default);
    Task<IReadOnlyList<ShareRequest>> GetByRecieverAsync(int recieverId, CancellationToken ct = default);
    Task<IReadOnlyList<ShareRequest>> GetByDocumentAsync(int documentId, CancellationToken ct = default);
    Task<ShareRequest?> GetWithRelationsAsync(int shareRequestId, CancellationToken ct = default);
    Task<ShareRequest> CreateAsync(int ownerId, int recieverId, int documentId, CancellationToken ct = default);
    Task<bool> ExistsAsync(int ownerId, int recieverId, int documentId, CancellationToken ct = default);
    Task<bool> AcceptAsync(int shareRequestId, Permission permission, CancellationToken ct = default);
    Task<bool> DeclineAsync(int shareRequestId, CancellationToken ct = default);
}
