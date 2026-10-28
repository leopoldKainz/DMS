using DMS.DAL.Interfaces;

namespace DMS.DAL.Entities;

public class User : BaseEntity, IUpdateAuditable
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string Password { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedOn { get; set; }

    public ICollection<UserDocument> UserDocuments { get; set; } = new List<UserDocument>();
    public ICollection<ShareRequest> OwnedShareRequests { get; set; } = new List<ShareRequest>();
    public ICollection<ShareRequest> ReceivedShareRequests { get; set; } = new List<ShareRequest>();
}
