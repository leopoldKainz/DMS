namespace DMS.DAL.Entities;

public class ShareRequest : IEntity, IAuditable
{
    public int Id { get; set; }
    public Guid EntityId { get; set;} = Guid.NewGuid();
    public User Owner { get; set; }
    public User Reciever { get; set; }
    public Document Document { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedOn { get; set; }
}