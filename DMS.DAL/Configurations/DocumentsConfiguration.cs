using DMS.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DMS.DAL.Configurations;

public abstract class UserConfiguration : BaseEntityConfiguration<User>
{
    public virtual void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedOnAdd();

        builder.Property(e => e.EntityId).IsRequired();
        builder.HasIndex(e => e.EntityId).IsUnique();
        
        builder.Property(e => e.Username).IsRequired();
        builder.Property(e => e.Username).HasMaxLength(20);

        builder.Property(e => e.Email).IsRequired();
        builder.Property(e => e.Email).HasMaxLength(50);
        
        builder.Property(e => e.Password).IsRequired();
        builder.Property(e => e.Password).HasMaxLength(100);
        
        builder.Property(e => e.CreatedAt).IsRequired();
        
        builder.Property(e => e.UpdatedOn).IsRequired(false);

    }
}