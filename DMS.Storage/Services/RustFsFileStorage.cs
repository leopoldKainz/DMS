using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using DMS.Storage.Exceptions;
using DMS.Storage.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DMS.Storage.Services;

public class RustFsFileStorage : IFileStorage
{
    private readonly IAmazonS3 _client;
    private readonly ILogger<RustFsFileStorage> _logger;
    private readonly string _bucketName;
    private readonly SemaphoreSlim _bucketLock = new(1, 1);
    private bool _bucketReady;

    public RustFsFileStorage(IAmazonS3 client, IOptions<RustFsOptions> options, ILogger<RustFsFileStorage> logger)
    {
        _client = client;
        _logger = logger;
        _bucketName = options.Value.BucketName;
    }

    public async Task UploadAsync(Guid documentId, Stream content, long size, string contentType, CancellationToken ct = default)
    {
        try
        {
            await EnsureBucketAsync(ct);
            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = documentId.ToString(),
                InputStream = content,
                ContentType = contentType,
                AutoCloseStream = false
            };
            request.Headers.ContentLength = size;
            await _client.PutObjectAsync(request, ct);

            _logger.LogInformation("Stored file for document {DocumentId} in bucket {Bucket}", documentId, _bucketName);
        }
        catch (Exception ex) when (ex is not OperationCanceledException and not StorageException)
        {
            throw new StorageException($"Could not store file for document {documentId}.", ex);
        }
    }

    public async Task<StoredFile?> DownloadAsync(Guid documentId, CancellationToken ct = default)
    {
        try
        {
            await EnsureBucketAsync(ct);
            using var response = await _client.GetObjectAsync(_bucketName, documentId.ToString(), ct);

            var buffer = new MemoryStream();
            await response.ResponseStream.CopyToAsync(buffer, ct);
            buffer.Position = 0;

            return new StoredFile(buffer, response.Headers.ContentType, response.ContentLength);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("File for document {DocumentId} not found in bucket {Bucket}", documentId, _bucketName);
            return null;
        }
        catch (Exception ex) when (ex is not OperationCanceledException and not StorageException)
        {
            throw new StorageException($"Could not load file for document {documentId}.", ex);
        }
    }

    public async Task DeleteAsync(Guid documentId, CancellationToken ct = default)
    {
        try
        {
            await EnsureBucketAsync(ct);
            await _client.DeleteObjectAsync(_bucketName, documentId.ToString(), ct);

            _logger.LogInformation("Deleted file for document {DocumentId} from bucket {Bucket}", documentId, _bucketName);
        }
        catch (Exception ex) when (ex is not OperationCanceledException and not StorageException)
        {
            throw new StorageException($"Could not delete file for document {documentId}.", ex);
        }
    }

    private async Task EnsureBucketAsync(CancellationToken ct)
    {
        if (_bucketReady)
        {
            return;
        }

        await _bucketLock.WaitAsync(ct);
        try
        {
            if (_bucketReady)
            {
                return;
            }

            try
            {
                await _client.PutBucketAsync(new PutBucketRequest { BucketName = _bucketName }, ct);
                _logger.LogInformation("Bucket {Bucket} is ready", _bucketName);
            }
            catch (AmazonS3Exception ex) when (ex.ErrorCode is "BucketAlreadyOwnedByYou" or "BucketAlreadyExists")
            {
                _logger.LogDebug("Bucket {Bucket} already exists", _bucketName);
            }

            _bucketReady = true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new StorageException($"Could not prepare bucket {_bucketName}.", ex);
        }
        finally
        {
            _bucketLock.Release();
        }
    }
}
