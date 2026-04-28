using DCRManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DCRManagement.Infrastructure.Persistence.Configurations;

public class ApprovalHistoryConfiguration : IEntityTypeConfiguration<ApprovalHistory>
{
    public void Configure(EntityTypeBuilder<ApprovalHistory> builder)
    {
        builder.ToTable("ApprovalHistories");
        builder.HasKey(h => h.Id);

        builder.Property(h => h.Comment).HasMaxLength(2000);
        builder.Property(h => h.Action).HasConversion<string>();
        builder.Property(h => h.FromStatus).HasConversion<string>();
        builder.Property(h => h.ToStatus).HasConversion<string>();

        builder.HasOne(h => h.Actor)
               .WithMany(u => u.ApprovalHistories)
               .HasForeignKey(h => h.ActorId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}