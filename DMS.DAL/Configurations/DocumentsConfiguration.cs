using DMS.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DMS.DAL.Configurations;

public class DocumentsConfiguration : BaseEntityConfiguration<Document>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("documents");

        builder.Property(x => x.Name).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(255);

        builder.Property(x => x.Type).IsRequired();
        builder.Property(x => x.Type).HasMaxLength(100);

        builder.Property(x => x.Size).IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedOn).IsRequired(false);
    }
}
