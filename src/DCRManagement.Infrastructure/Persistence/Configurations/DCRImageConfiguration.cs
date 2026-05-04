using DCRManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DCRManagement.Infrastructure.Persistence.Configurations;

public class DCRImageConfiguration : IEntityTypeConfiguration<DCRImage>
{
    public void Configure(EntityTypeBuilder<DCRImage> builder)
    {
        builder.ToTable("DCRImages");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.FileName)
               .IsRequired()
               .HasMaxLength(260);

        builder.Property(i => i.ContentType)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(i => i.Caption)
               .HasMaxLength(500);

        builder.Property(i => i.Category)
               .HasConversion<string>()
               .HasMaxLength(20);

        builder.Property(i => i.GalleryType)
               .IsRequired()
               .HasMaxLength(10);   // "Before" | "After"

        // Layout preservation columns
        builder.Property(i => i.DisplayOrder).HasDefaultValue(0);
        builder.Property(i => i.ThumbnailWidth).HasDefaultValue(140);
        builder.Property(i => i.ThumbnailHeight).HasDefaultValue(140);

        // Binary stored directly in SQL — no size limit
        builder.Property(i => i.ImageData)
               .IsRequired()
               .HasColumnType("VARBINARY(MAX)");

        // Composite index: fast lookup by DCR + gallery type + order
        builder.HasIndex(i => new { i.DCRId, i.GalleryType, i.DisplayOrder });

        builder.HasOne(i => i.DCR)
               .WithMany(d => d.Images)
               .HasForeignKey(i => i.DCRId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
