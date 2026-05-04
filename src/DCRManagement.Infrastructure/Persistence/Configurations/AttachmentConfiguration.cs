using DCRManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DCRManagement.Infrastructure.Persistence.Configurations;

public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.ToTable("Attachments");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.FileName).IsRequired().HasMaxLength(500);
        builder.Property(a => a.StoredFileName).IsRequired().HasMaxLength(100);
        builder.Property(a => a.FilePath).IsRequired().HasMaxLength(1000);
        builder.Property(a => a.ContentType).IsRequired().HasMaxLength(100);

        // Gallery image columns (nullable — null means regular file attachment)
        builder.Property(a => a.ImageType).HasMaxLength(10).IsRequired(false);
        builder.Property(a => a.DisplayOrder).IsRequired(false);

        // Index to speed up gallery queries: WHERE DCRId = ? AND ImageType = ?
        builder.HasIndex(a => new { a.DCRId, a.ImageType });
    }
}
