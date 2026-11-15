namespace DMS.ELK;

public record SearchDocument
{
    public Guid DocumentId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
