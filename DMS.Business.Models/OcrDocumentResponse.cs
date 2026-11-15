using DMS.DAL.Enums;

namespace DMS.Business.Models;

public record OcrDocumentResponse
{
    public Guid JobId { get; init; }
    public Guid DocumentId { get; init; }
    public OcrStatus Status { get; init; }
    public string? ExtractedText { get; init; }
    public string? ErrorMessage { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
