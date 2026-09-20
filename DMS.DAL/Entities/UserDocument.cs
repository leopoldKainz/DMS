using DMS.DAL.Enums;

namespace DMS.DAL.Entities;

public class UserDocument: IEntity, IAuditable
{
    public int Id { get; set; }
    public Guid EntityId { get; set;} = Guid.NewGuid();
    public User User { get; set; }
    public Permission Permission { get; set; }
    public Document Document { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedOn { get; set; }
}