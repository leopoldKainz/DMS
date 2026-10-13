using DMS.Business.Interfaces;
using DMS.DAL;
using DMS.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DMS.Business.Services;

public abstract class BaseService<T> : IBaseService<T> where T : BaseEntity
{
    protected readonly DmsDbContext _context;
    protected readonly DbSet<T> _dbSet;
    protected readonly ILogger _logger;

    protected BaseService(DmsDbContext context, ILogger logger)
    {
        _context = context;
        _dbSet = context.Set<T>();
        _logger = logger;
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.EntityId == id, ct);
    }

    public virtual async Task<T?> GetByEntityIdAsync(Guid entityId, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.EntityId == entityId, ct);
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
    {
        return await _dbSet.AsNoTracking().ToListAsync(ct);
    }

    public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet.AnyAsync(e => e.EntityId == id, ct);
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await _dbSet.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("{EntityType} {EntityId} created", typeof(T).Name, entity.EntityId);
        return entity;
    }

    public virtual async Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("{EntityType} {EntityId} updated", typeof(T).Name, entity.EntityId);
    }

    public virtual async Task RemoveAsync(T entity, CancellationToken ct = default)
    {
        _dbSet.Remove(entity);
        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("{EntityType} {EntityId} removed", typeof(T).Name, entity.EntityId);
    }

    public virtual async Task<bool> RemoveByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _dbSet.FirstOrDefaultAsync(e => e.EntityId == id, ct);
        if (entity == null)
        {
            _logger.LogDebug("{EntityType} {EntityId} not found for removal", typeof(T).Name, id);
            return false;
        }

        _dbSet.Remove(entity);
        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("{EntityType} {EntityId} removed", typeof(T).Name, id);
        return true;
    }
}
