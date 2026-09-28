using DMS.DAL.Entities;

namespace DMS.Business.Interfaces;
public interface IBaseService<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<T?> GetByEntityIdAsync(Guid entityId, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
    Task<T> AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
    Task<bool> RemoveByIdAsync(int id, CancellationToken ct = default);
}
