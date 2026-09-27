using System.Linq.Expressions;
using DMS.DAL.Entities;

namespace DMS.DAL.Repositories.Interfaces;

// Generic read access for every entity deriving from BaseEntity.
public interface IBaseRepository<T> where T : class
{
    Task<T?> GetByIdAsync(object id, CancellationToken ct = default);
    Task<List<T>> ListAsync(CancellationToken ct = default);
    Task<List<T>> ListAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
