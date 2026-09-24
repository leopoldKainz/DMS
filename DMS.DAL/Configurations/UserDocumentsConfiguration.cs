using DMS.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DMS.DAL.Configurations;
public class UserDocumentsConfiguration : IEntityTypeConfiguration<UserDocument>
{
    public void Configure(EntityTypeBuilder<UserDocument> builder)
    {
        builder.ToTable("user_documents");

        builder.HasKey(x => new { x.UserId, x.DocumentId });

        builder.HasOne(x => x.User)
               .WithMany(x => x.UserDocuments)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Document)
               .WithMany(x => x.UserDocuments)
               .HasForeignKey(x => x.DocumentId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.Permission).IsRequired();
        builder.Property(x => x.Permission).HasConversion<string>();
        builder.Property(x => x.Permission).HasMaxLength(20);

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedOn).IsRequired(false);
    }
}
