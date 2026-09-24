using DMS.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DMS.DAL.Configurations;

public class ShareRequestsConfiguration : BaseEntityConfiguration<ShareRequest>
{
    protected override void ConfigureEntity(EntityTypeBuilder<ShareRequest> builder)
    {
        builder.ToTable("share_requests");

        builder.Property<int>("OwnerId");
        builder.Property<int>("RecieverId");
        builder.Property<int>("DocumentId");

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

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedOn).IsRequired(false);
    }
}
