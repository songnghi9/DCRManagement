using BCrypt.Net;
using DCRManagement.Domain.Entities;
using DCRManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DCRManagement.Infrastructure.Persistence;

/// <summary>
/// Seeds initial data for development/production first-run.
/// Safe to run multiple times — checks for existence before inserting.
/// </summary>
public class DbSeeder
{
    private readonly AppDbContext _context;
    private readonly ILogger<DbSeeder> _logger;

    public DbSeeder(AppDbContext context, ILogger<DbSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        await _context.Database.MigrateAsync();

        if (await _context.Users.AnyAsync()) return;

        _logger.LogInformation("Seeding initial users...");

        var users = new List<User>
        {
            new() {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                FullName = "System Administrator",
                Email = "admin@company.com",
                Role = UserRole.Admin,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedById = 1
            },
            new() {
                Username = "engineer01",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Engineer@123"),
                FullName = "Nguyen Van A",
                Email = "engineer01@company.com",
                Role = UserRole.Engineer,
                Department = "R&D",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedById = 1
            },
            new() {
                Username = "reviewer01",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Reviewer@123"),
                FullName = "Tran Thi B",
                Email = "reviewer01@company.com",
                Role = UserRole.Reviewer,
                Department = "QA",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedById = 1
            },
            new() {
                Username = "approver01",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Approver@123"),
                FullName = "Le Van C",
                Email = "approver01@company.com",
                Role = UserRole.Approver,
                Department = "Management",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedById = 1
            }
        };

        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} users.", users.Count);
    }
}
