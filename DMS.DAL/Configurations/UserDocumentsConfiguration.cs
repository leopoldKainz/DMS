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

        builder.Property(x => x.UserId).HasColumnName("user_id");
        builder.Property(x => x.DocumentId).HasColumnName("document_id");

        builder.HasOne(x => x.User)
               .WithMany(x => x.UserDocuments)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Document)
               .WithMany(x => x.UserDocuments)
               .HasForeignKey(x => x.DocumentId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.Permission).HasColumnName("permission").IsRequired();
        builder.Property(x => x.Permission).HasConversion<string>();
        builder.Property(x => x.Permission).HasMaxLength(20);

        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.UpdatedOn).HasColumnName("updated_on").IsRequired(false);
    }
}
