namespace DMS.Storage.Interfaces;

public interface IFileStorage
{
    Task UploadAsync(Guid documentId, Stream content, long size, string contentType, CancellationToken ct = default);
    Task<StoredFile?> DownloadAsync(Guid documentId, CancellationToken ct = default);
    Task DeleteAsync(Guid documentId, CancellationToken ct = default);
}
