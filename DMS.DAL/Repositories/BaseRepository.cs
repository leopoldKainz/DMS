using System.Linq.Expressions;
using DMS.DAL.Entities;
using DMS.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DMS.DAL.Repositories;

public class Repository<T> : IBaseRepository<T> where T : class
{
    protected readonly DmsDbContext _context;
    protected readonly DbSet<T> _set;

    public Repository(DmsDbContext context)
    {
        _context = context;
        _set = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(object id, CancellationToken ct = default)
        => await _set.FindAsync(new[] { id }, ct);

    public Task<List<T>> ListAsync(CancellationToken ct = default)
        => _set.AsNoTracking().ToListAsync(ct);

    public Task<List<T>> ListAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => _set.AsNoTracking().Where(predicate).ToListAsync(ct);

    public async Task AddAsync(T entity, CancellationToken ct = default)
        => await _set.AddAsync(entity, ct);

    public void Update(T entity) => _set.Update(entity);

    public void Remove(T entity) => _set.Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}