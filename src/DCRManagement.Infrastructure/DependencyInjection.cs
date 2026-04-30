using DCRManagement.Application.Common;
using DCRManagement.Domain.Interfaces;
using DCRManagement.Infrastructure.Persistence;
using DCRManagement.Infrastructure.Persistence.Repositories;
using DCRManagement.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DCRManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<IDCRRepository, DCRRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IApprovalHistoryRepository, ApprovalHistoryRepository>();
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));

        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<AttachmentService>();
        services.AddScoped<IGalleryImageService>(sp => sp.GetRequiredService<AttachmentService>());
        services.AddScoped<DbSeeder>();

        return services;
    }
}