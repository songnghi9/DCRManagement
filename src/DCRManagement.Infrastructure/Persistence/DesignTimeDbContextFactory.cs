using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DCRManagement.Infrastructure.Persistence;

/// <summary>
/// Design-time DbContext factory for EF Core CLI tools (migrations, etc.)
/// This allows the tools to create a DbContext instance without running the full application.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // Use SQL Server Express for design-time operations
        optionsBuilder.UseSqlServer(
            "Server=localhost\\SQLEXPRESS;Database=DCRManagement;Trusted_Connection=true;Encrypt=false;",
            sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));

        return new AppDbContext(optionsBuilder.Options);
    }
}
