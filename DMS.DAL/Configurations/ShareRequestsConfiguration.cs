using DMS.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DMS.DAL.Configurations;

public class ShareRequestsConfiguration : BaseEntityConfiguration<ShareRequest>
{
    protected override void ConfigureEntity(EntityTypeBuilder<ShareRequest> builder)
    {
        builder.ToTable("share_requests");

        builder.Property<int>("OwnerId").HasColumnName("owner_id");
        builder.Property<int>("RecieverId").HasColumnName("reciever_id");
        builder.Property<int>("DocumentId").HasColumnName("document_id");

        builder.HasOne(x => x.Owner)
               .WithMany(x => x.OwnedShareRequests)
               .HasForeignKey("OwnerId")
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Reciever)
               .WithMany(x => x.ReceivedShareRequests)
               .HasForeignKey("RecieverId")
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Document)
               .WithMany(x => x.ShareRequests)
               .HasForeignKey("DocumentId")
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex("OwnerId", "RecieverId", "DocumentId").IsUnique();

        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.UpdatedOn).HasColumnName("updated_on").IsRequired(false);
    }
}
