using DCRManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DCRManagement.Infrastructure.Persistence.Configurations;

public class DCRConfiguration : IEntityTypeConfiguration<DCR>
{
    public void Configure(EntityTypeBuilder<DCR> builder)
    {
        builder.ToTable("DCRs");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.DCRNumber).IsRequired().HasMaxLength(20);
        builder.Property(d => d.Title).IsRequired().HasMaxLength(300);
        builder.Property(d => d.Description).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(d => d.AffectedParts).HasColumnType("nvarchar(max)");
        builder.Property(d => d.Reason).HasColumnType("nvarchar(max)");
        builder.Property(d => d.ImpactAnalysis).HasColumnType("nvarchar(max)");
        builder.Property(d => d.Priority).HasMaxLength(20);
        builder.Property(d => d.Status).HasConversion<string>(); // Store as readable string

        builder.HasIndex(d => d.DCRNumber).IsUnique();
        builder.HasIndex(d => d.Status);

        builder.HasOne(d => d.AssignedReviewer)
               .WithMany()
               .HasForeignKey(d => d.AssignedReviewerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.AssignedApprover)
               .WithMany()
               .HasForeignKey(d => d.AssignedApproverId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(d => d.ApprovalHistories)
               .WithOne(h => h.DCR)
               .HasForeignKey(h => h.DCRId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.Attachments)
               .WithOne(a => a.DCR)
               .HasForeignKey(a => a.DCRId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
