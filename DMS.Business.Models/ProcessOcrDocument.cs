namespace DMS.Business.Models;

public record ProcessOcrDocument
{
    public Guid JobId { get; set; }
    public Guid DocumentId { get; set; }
    public string? DocumentName { get; set; }
}
