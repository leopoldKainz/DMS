using DMS.DAL.Interfaces;

namespace DMS.DAL.Entities;

public class ShareRequest : BaseEntity, IUpdateAuditable
{
    public User Owner { get; set; }
    public User Reciever { get; set; }
    public Document Document { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedOn { get; set; }
}