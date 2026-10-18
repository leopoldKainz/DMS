using MassTransit;
using Microsoft.Extensions.Logging;

namespace DMS.Business.Models;

public class OcrWorkerConsumer: IConsumer<ProcessOcrDocument>
{
    private readonly ILogger<OcrWorkerConsumer> _logger;
    public OcrWorkerConsumer(ILogger<OcrWorkerConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProcessOcrDocument> context)
    {
        var message = context.Message;

        //TODO: Implement the actual OCR processing logic here. For now, we will just log the received message and simulate some work.
        
        _logger.LogInformation($"Starting OCR for Job {message.JobId} using fileID {message.DocumentId}");
        await Task.Delay(500);
        _logger.LogInformation($"Successfully completed OCR for Job {message.JobId} using fileID {message.DocumentId}");
    }
}