namespace DMS.Business.Models;

public record ProcessOcrDocumentResponse
{
    public Guid JobId { get; set; }
    public Guid DocumentId { get; set; }
    public bool Success { get; set; }
    public string? ExtractedText { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ProcessedAt { get; set; }
}
