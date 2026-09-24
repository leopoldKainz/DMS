using DMS.DAL.Enums;
using DMS.DAL.Interfaces;

namespace DMS.DAL.Entities;

public class UserDocument: IUpdateAuditable
{
    public User User { get; set; }
    public Permission Permission { get; set; }
    public Document Document { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedOn { get; set; }
}