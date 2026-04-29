using DCRManagement.Application;
using DCRManagement.Infrastructure;
using DCRManagement.Infrastructure.Persistence;
using DCRManagement.UI.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DCRManagement.UI;

static class Program
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    [STAThread]
    static async Task Main()
    {
        ApplicationConfiguration.Initialize();

        var services = new ServiceCollection();

        var config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        services.AddSingleton<IConfiguration>(config);
        services.AddLogging(b => b.AddConsole());

        services.AddInfrastructure(config);
        services.AddApplication();

        services.AddTransient<LoginForm>();
        services.AddTransient<MainForm>();
        services.AddTransient<DCRListForm>();
        services.AddTransient<DCRDetailForm>();
        services.AddTransient<UserManagementForm>();

        ServiceProvider = services.BuildServiceProvider();

        using (var scope = ServiceProvider.CreateScope())
        {
            var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
            await seeder.SeedAsync();
        }

        var loginForm = ServiceProvider.GetRequiredService<LoginForm>();
        System.Windows.Forms.Application.Run(loginForm);  // fully qualify để tránh nhầm namespace
    }
}