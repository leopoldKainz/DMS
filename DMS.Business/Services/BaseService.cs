using DMS.Business.Interfaces;
using DMS.DAL;
using DMS.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DMS.Business.Services;

public abstract class BaseService<T> : IBaseService<T> where T : BaseEntity
{
    protected readonly DmsDbContext _context;
    protected readonly DbSet<T> _dbSet;

    protected BaseService(DmsDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public virtual async Task<T?> GetByEntityIdAsync(Guid entityId, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.EntityId == entityId, ct);
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
    {
        return await _dbSet.AsNoTracking().ToListAsync(ct);
    }

    public virtual async Task<bool> ExistsAsync(int id, CancellationToken ct = default)
    {
        return await _dbSet.AnyAsync(e => e.Id == id, ct);
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await _dbSet.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
        return entity;
    }

    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
        _context.SaveChanges();
    }

    public virtual void Remove(T entity)
    {
        _dbSet.Remove(entity);
        _context.SaveChanges();
    }

    public virtual async Task<bool> RemoveByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _dbSet.FirstOrDefaultAsync(e => e.Id == id, ct);
        if (entity == null)
        {
            return false;
        }

        _dbSet.Remove(entity);
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
