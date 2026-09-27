namespace DMS.Business.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserService UserService { get; }
    IDocumentService DocumentService { get; }
    IShareRequestService ShareRequestService { get; }
    IUserDocumentService UserDocumentService { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}