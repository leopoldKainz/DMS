using MassTransit;
using Microsoft.Extensions.Logging;

namespace DMS.Business.Models;

public class OcrWorkerConsumer : IConsumer<ProcessOcrDocument>
{
    private readonly ILogger<OcrWorkerConsumer> _logger;
    public OcrWorkerConsumer(ILogger<OcrWorkerConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProcessOcrDocument> context)
    {
        var message = context.Message;
        try
        {
            //TODO: Implement the OCR logic here.
            _logger.LogInformation("Starting OCR for Job {JobId} using fileID {DocumentId}", message.JobId, message.DocumentId);
            await Task.Delay(500);
            _logger.LogInformation("Successfully completed OCR for Job {JobId} using fileID {DocumentId}", message.JobId, message.DocumentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OCR failed for Job {JobId} using fileID {DocumentId}", message.JobId, message.DocumentId);
            throw;
        }
    }
}
