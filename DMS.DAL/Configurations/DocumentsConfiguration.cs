using DMS.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DMS.DAL.Configurations;

public class DocumentsConfiguration : BaseEntityConfiguration<Document>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("documents");

        builder.Property(x => x.Name).HasColumnName("name").IsRequired().HasMaxLength(255);
        builder.Property(x => x.Type).HasColumnName("type").IsRequired().HasMaxLength(100);
        builder.Property(x => x.Size).HasColumnName("size").IsRequired();

        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.UpdatedOn).HasColumnName("updated_on").IsRequired(false);
    }
}
