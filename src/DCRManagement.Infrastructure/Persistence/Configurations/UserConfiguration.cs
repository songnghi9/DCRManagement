using DCRManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DCRManagement.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Username).IsRequired().HasMaxLength(50);
        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(256);
        builder.Property(u => u.FullName).IsRequired().HasMaxLength(150);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(200);
        builder.Property(u => u.Department).HasMaxLength(100);

        builder.HasIndex(u => u.Username).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();

        builder.HasMany(u => u.CreatedDCRs)
               .WithOne()
               .HasForeignKey(d => d.CreatedById)
               .OnDelete(DeleteBehavior.Restrict);
    }
}