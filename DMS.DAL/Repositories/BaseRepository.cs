using DMS.DAL.Entities;
using DMS.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DMS.DAL.Repositories;

public class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity
{
    protected readonly DmsDbContext Context;

    public BaseRepository(DmsDbContext context)
    {
        Context = context;
    }

    public async Task<IReadOnlyList<T>> GetAll(CancellationToken cancellationToken = default)
    {
        return await Context.Set<T>().ToListAsync(cancellationToken);
    }

    public Task<T?> GetById(int id, CancellationToken cancellationToken = default)
    {
        return Context.Set<T>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}
