namespace DMS.Business.Interfaces;
namespace DMS.Business.Models;

public interface IShareRequestService
{
    Task<ShareRequest> CreateShareRequestAsync(int documentId, int userId, Permission permission, CancellationToken ct = default);
    Task<ShareRequest?> GetShareRequestByIdAsync(int shareRequestId, CancellationToken ct = default);
    Task<List<ShareRequest>> GetShareRequestsByUserIdAsync(int userId, CancellationToken ct = default);
    Task<List<ShareRequest>> GetShareRequestsByDocumentIdAsync(int documentId, CancellationToken ct = default);
    Task UpdateShareRequestStatusAsync(int shareRequestId, ShareRequestStatus status, CancellationToken ct = default);
}