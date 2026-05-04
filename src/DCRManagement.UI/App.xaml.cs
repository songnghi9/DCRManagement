using DCRManagement.Application;
using DCRManagement.Infrastructure;
using DCRManagement.Infrastructure.Persistence;
using DCRManagement.UI.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace DCRManagement.UI;

public partial class App : System.Windows.Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += (_, args) =>
        {
            MessageBox.Show(args.Exception.Message, "Unexpected Error", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };

        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        services.AddSingleton<IConfiguration>(config);
        services.AddLogging(builder => builder.AddConsole());
        services.AddInfrastructure(config);
        services.AddApplication();
        services.AddTransient<LoginWindow>();
        // MainWindow needs IServiceProvider — register as factory
        services.AddTransient<MainWindow>(sp =>
            new MainWindow(sp, sp.GetRequiredService<DCRManagement.Application.Services.AuthService>()));
        services.AddTransient<DCRListView>();
        services.AddTransient<DCRDetailView>();
        services.AddTransient<UserManagementView>();

        Services = services.BuildServiceProvider();

        ShutdownMode = ShutdownMode.OnLastWindowClose;
        var loginWindow = Services.GetRequiredService<LoginWindow>();
        MainWindow = loginWindow;
        loginWindow.Show();

        _ = SeedDatabaseAsync();
    }

    private static async Task SeedDatabaseAsync()
    {
        try
        {
            using var scope = Services.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
            await seeder.SeedAsync();
        }
        catch (Exception ex)
        {
            Current.Dispatcher.Invoke(() =>
                MessageBox.Show(
                    $"The UI is running, but database initialization failed:\n\n{ex.Message}",
                    "Database Initialization",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning));
        }
    }
}
