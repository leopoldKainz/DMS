using DMS.DAL.Entities;

namespace DMS.DAL.Repositories.Interfaces;

// Generic read access for every entity deriving from BaseEntity.
public interface IBaseRepository<T> where T : BaseEntity
{
    Task<IReadOnlyList<T>> GetAll(CancellationToken cancellationToken = default);

    // Looks up by the int primary key, not by EntityId.
    Task<T?> GetById(int id, CancellationToken cancellationToken = default);
}
