using DMS.DAL.Enums;
using DMS.DAL.Interfaces;

namespace DMS.DAL.Entities;

public class Document : BaseEntity, IUpdateAuditable
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public long Size { get; set; }
    public OcrStatus OcrStatus { get; set; } = OcrStatus.Pending;
    public DocumentMetadata Metadata { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedOn { get; set; }

    public ICollection<UserDocument> UserDocuments { get; set; } = new List<UserDocument>();
    public ICollection<ShareRequest> ShareRequests { get; set; } = new List<ShareRequest>();
}
