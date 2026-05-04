using DCRManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DCRManagement.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<DCR> DCRs { get; set; }
    public DbSet<ApprovalHistory> ApprovalHistories { get; set; }
    public DbSet<Attachment> Attachments { get; set; }
    public DbSet<DCRImage> DCRImages { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all IEntityTypeConfiguration<T> classes in this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Global soft-delete filter — IsDeleted=true records are invisible app-wide
        modelBuilder.Entity<DCR>().HasQueryFilter(d => !d.IsDeleted);
        modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<Attachment>().HasQueryFilter(a => !a.IsDeleted);
    }
}