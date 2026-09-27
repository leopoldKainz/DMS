namespace DMS.Business.Models;

public class ShareRequest
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public int UserId { get; set; }
    public Permission Permission { get; set; }
    public ShareRequestStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedOn { get; set; }
}