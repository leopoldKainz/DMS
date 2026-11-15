using DMS.Business.Models;
using DMS.DAL;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DMS.API.Consumers;

public class OcrResponseConsumer : IConsumer<OcrDocumentResponse>
{
    private readonly DmsDbContext _dbContext;
    private readonly ILogger<OcrResponseConsumer> _logger;

    public OcrResponseConsumer(DmsDbContext dbContext, ILogger<OcrResponseConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OcrDocumentResponse> context)
    {
        var message = context.Message;
        _logger.LogInformation("Received OCR response for document {DocumentId}: Status = {Status}", message.DocumentId, message.Status);

        var document = await _dbContext.Documents.FirstOrDefaultAsync(d => d.EntityId == message.DocumentId);
        if (document == null)
        {
            _logger.LogWarning("Document {DocumentId} not found when updating OCR status to {Status}", message.DocumentId, message.Status);
            return;
        }

        document.OcrStatus = message.Status;
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Successfully updated document {DocumentId} OCR status to {Status}", message.DocumentId, message.Status);
    }
}
