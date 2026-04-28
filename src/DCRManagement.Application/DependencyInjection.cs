using DCRManagement.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DCRManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        services.AddScoped<DCRService>();
        services.AddScoped<WorkflowService>();
        services.AddScoped<UserService>();
        return services;
    }
}