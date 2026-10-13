using DMS.DAL.Entities;

namespace DMS.Business.Interfaces;

public interface IBaseService<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<T?> GetByEntityIdAsync(Guid entityId, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task<T> AddAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task RemoveAsync(T entity, CancellationToken ct = default);
    Task<bool> RemoveByIdAsync(Guid id, CancellationToken ct = default);
}
