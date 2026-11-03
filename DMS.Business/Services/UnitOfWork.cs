using DMS.Business.Interfaces;
using DMS.DAL;
using DMS.DAL.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DMS.Business.Services;

public class UnitOfWork : IUnitOfWork
{
    private readonly DmsDbContext _context;
    private readonly ILogger<UnitOfWork> _logger;

    public UnitOfWork(DmsDbContext context, ILogger<UnitOfWork> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            return await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Saving changes to the database failed");
            throw new DataAccessException("Saving changes to the database failed.", ex);
        }
    }
}
