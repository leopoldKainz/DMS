namespace DMS.DAL.DTOs;

public record DocumentDto(
    Guid Id,
    string Name,
    string Type,
    long Size,
    DateTime CreatedAt
)
{
    public DateTime? UpdatedOn { get; init; }
    public string OcrStatus { get; init; } = string.Empty;
    public DocumentMetadataDto Metadata { get; init; } = new(null, null);
}
