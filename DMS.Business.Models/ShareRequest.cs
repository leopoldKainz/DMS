using DMS.DAL.Enums;

namespace DMS.Business.Models;

public class ShareRequest
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public Guid UserId { get; set; }
    public Permission Permission { get; set; }
    public ShareRequestStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedOn { get; set; }
}
