using Microsoft.EntityFrameworkCore;

namespace DMS.DAL.Entities;

public class BaseEntity : IEntity
{
    public int Id { get; set; }
    public Guid EntityId { get; set; }
}