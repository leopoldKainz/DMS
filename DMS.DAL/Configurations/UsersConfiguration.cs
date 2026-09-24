using DMS.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DMS.DAL.Configurations;

public class UsersConfiguration : BaseEntityConfiguration<User>
{
    protected override void ConfigureEntity(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.Property(x => x.Username).IsRequired();
        builder.Property(x => x.Username).HasMaxLength(20);
        builder.HasIndex(x => x.Username).IsUnique();

        builder.Property(x => x.Email).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(50);
        builder.HasIndex(x => x.Email).IsUnique();

        builder.Property(x => x.Password).IsRequired();
        builder.Property(x => x.Password).HasMaxLength(100);

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedOn).IsRequired(false);
    }
}
