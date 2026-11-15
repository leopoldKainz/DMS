using DMS.Business.Models;
using DMS.DAL.Enums;
using DMS.ELK;
using DMS.ELK.Interfaces;
using DMS.OCR.Services;
using DMS.Storage.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace DMS.OCR.Consumers;

public class OcrWorkerConsumer : IConsumer<ProcessOcrDocument>
{
    private readonly IFileStorage _fileStorage;
    private readonly ISearchIndex _searchIndex;
    private readonly IOcrService _ocrService;
    private readonly ILogger<OcrWorkerConsumer> _logger;

    public OcrWorkerConsumer(
        IFileStorage fileStorage,
        ISearchIndex searchIndex,
        IOcrService ocrService,
        ILogger<OcrWorkerConsumer> logger)
    {
        _fileStorage = fileStorage;
        _searchIndex = searchIndex;
        _ocrService = ocrService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProcessOcrDocument> context)
    {
        var message = context.Message;
        _logger.LogInformation("Received OCR processing request for Job {JobId}, Document {DocumentId}",
            message.JobId, message.DocumentId);

        // 1. Notify that we have started working on the document
        await context.Publish(new OcrDocumentResponse
        {
            JobId = message.JobId,
            DocumentId = message.DocumentId,
            Status = OcrStatus.Processing,
            Timestamp = DateTime.UtcNow
        });

        var tempPdfPath = Path.Combine(Path.GetTempPath(), $"{message.DocumentId}_{Guid.NewGuid():N}.pdf");

        try
        {
            // 2. Fetch the original PDF from RustFS
            _logger.LogInformation("Downloading PDF for document {DocumentId} from RustFS", message.DocumentId);
            var storedFile = await _fileStorage.DownloadAsync(message.DocumentId, context.CancellationToken);
            if (storedFile == null)
            {
                throw new FileNotFoundException($"File for document {message.DocumentId} not found in storage.");
            }

            await using (var fileStream = File.Create(tempPdfPath))
            {
                await storedFile.Content.CopyToAsync(fileStream, context.CancellationToken);
            }

            // 3. Perform OCR using Ghostscript and Tesseract (supporting German, English, and handwriting)
            _logger.LogInformation("Performing OCR on document {DocumentId}", message.DocumentId);
            var extractedText = await _ocrService.ProcessPdfAsync(tempPdfPath, context.CancellationToken);
            _logger.LogInformation("OCR extracted {Length} characters for document {DocumentId}",
                extractedText.Length, message.DocumentId);

            // 4. Save the OCR result into Elasticsearch for later use
            var searchDoc = new SearchDocument
            {
                DocumentId = message.DocumentId,
                Name = message.DocumentName ?? message.DocumentId.ToString(),
                Content = extractedText,
                CreatedAt = DateTime.UtcNow
            };
            await _searchIndex.IndexAsync(searchDoc, context.CancellationToken);
            _logger.LogInformation("Indexed document {DocumentId} into Elasticsearch", message.DocumentId);

            // 5. Send completion status response back through the response queue
            await context.Publish(new OcrDocumentResponse
            {
                JobId = message.JobId,
                DocumentId = message.DocumentId,
                Status = OcrStatus.Done,
                ExtractedText = extractedText,
                Timestamp = DateTime.UtcNow
            });
            _logger.LogInformation("Successfully completed OCR processing for Job {JobId}, Document {DocumentId}",
                message.JobId, message.DocumentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed OCR processing for Job {JobId}, Document {DocumentId}",
                message.JobId, message.DocumentId);

            // Notify error through the response queue
            await context.Publish(new OcrDocumentResponse
            {
                JobId = message.JobId,
                DocumentId = message.DocumentId,
                Status = OcrStatus.Failed,
                ErrorMessage = ex.Message,
                Timestamp = DateTime.UtcNow
            });

            throw;
        }
        finally
        {
            if (File.Exists(tempPdfPath))
            {
                try
                {
                    File.Delete(tempPdfPath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not delete temporary PDF file {Path}", tempPdfPath);
                }
            }
        }
    }
}
