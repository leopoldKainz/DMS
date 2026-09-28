using DMS.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DMS.DAL.Configurations;

public class UsersConfiguration : BaseEntityConfiguration<User>
{
    protected override void ConfigureEntity(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.Property(x => x.Username).HasColumnName("username").IsRequired().HasMaxLength(20);
        builder.HasIndex(x => x.Username).IsUnique();

        builder.Property(x => x.Email).HasColumnName("email").IsRequired().HasMaxLength(50);
        builder.HasIndex(x => x.Email).IsUnique();

        builder.Property(x => x.Password).HasColumnName("password").IsRequired().HasMaxLength(100);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.UpdatedOn).HasColumnName("updated_on").IsRequired(false);
    }
}
