using DMS.DAL.Entities;
using DMS.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DMS.DAL;

public class DmsDbContext : DbContext
{
    public DmsDbContext(DbContextOptions<DmsDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<UserDocument> UserDocuments => Set<UserDocument>();
    public DbSet<ShareRequest> ShareRequests => Set<ShareRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DmsDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditTimestamps();

        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyAuditTimestamps();

        return base.SaveChanges();
    }

    private void ApplyAuditTimestamps()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<ICreateAuditable>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(x => x.CreatedAt).IsModified = false;
            }
        }

        foreach (var entry in ChangeTracker.Entries<IUpdateAuditable>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedOn = now;
            }
        }
    }
}
