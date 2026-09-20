
namespace DMS.DAL.Entities;
public class User : IEntity, IAuditable
{
    public int Id { get; set; }
    public Guid EntityId { get; set;} = Guid.NewGuid();
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string Password { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedOn { get; set; }
    
    public ICollection<UserDocument> UserDocuments { get; set; }
    public ICollection<ShareRequest> OwnedShareRequests { get; set; }
    public ICollection<ShareRequest> ReceivedShareRequests { get; set; }
}