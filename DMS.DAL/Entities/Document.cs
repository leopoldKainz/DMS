namespace DMS.DAL.Entities;

public class Document: IEntity, IAuditable
{
    public int Id { get; set; }
    public Guid EntityId { get; set;} = Guid.NewGuid();
    public string Name { get; set; }
    public string Type { get; set; }
    public Int64 Size { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedOn { get; set; }
    
    public ICollection<UserDocument> UserDocuments { get; set; }
    public ICollection<ShareRequest> ShareRequests { get; set; }
}