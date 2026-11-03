using MassTransit;
using Microsoft.Extensions.Logging;

namespace DMS.Business.Models;

public class OcrResponseConsumer : IConsumer<ProcessOcrDocumentResponse>
{
    private readonly ILogger<OcrResponseConsumer> _logger;

    public OcrResponseConsumer(ILogger<OcrResponseConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<ProcessOcrDocumentResponse> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Received OCR response for Job {JobId} (fileID {DocumentId}): Success={Success}, ProcessedAt={ProcessedAt}",
            message.JobId,
            message.DocumentId,
            message.Success,
            message.ProcessedAt);

        return Task.CompletedTask;
    }
}
