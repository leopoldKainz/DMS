using DMS.DAL.Interfaces;

namespace DMS.DAL.Entities;

public class Document: BaseEntity, IUpdateAuditable
{
    public string Name { get; set; }
    public string Type { get; set; }
    public long Size { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedOn { get; set; }
    
    public ICollection<UserDocument> UserDocuments { get; set; }
    public ICollection<ShareRequest> ShareRequests { get; set; }
}