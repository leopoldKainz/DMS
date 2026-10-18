using DMS.Business.Interfaces;
using DMS.DAL;

namespace DMS.Business.Services;

public class UnitOfWork : IUnitOfWork
{
    private readonly DmsDbContext _context;

    public UnitOfWork(DmsDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return _context.SaveChangesAsync(ct);
    }
}
