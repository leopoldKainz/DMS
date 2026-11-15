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

        builder.Property(x => x.OcrStatus).HasColumnName("ocr_status").IsRequired();
        builder.Property(x => x.OcrStatus).HasConversion<string>();
        builder.Property(x => x.OcrStatus).HasMaxLength(20);

        builder.OwnsOne(x => x.Metadata, metadata =>
        {
            metadata.ToJson("metadata");
            metadata.Property(m => m.ContentType).HasJsonPropertyName("contentType");
            metadata.Property(m => m.PageCount).HasJsonPropertyName("pageCount");
        });
        builder.Navigation(x => x.Metadata).IsRequired();

        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.UpdatedOn).HasColumnName("updated_on").IsRequired(false);
    }
}
