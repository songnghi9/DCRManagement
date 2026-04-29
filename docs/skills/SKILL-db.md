---
name: dcr-database
description: Quản lý EF Core migrations, seed dữ liệu ban đầu, và kết nối SQL Server cho DCRManagement. Dùng khi cần tạo migration mới, update database, reset dữ liệu dev, hoặc cấu hình connection string.
---

# Skill: Database Management

## Connection String

Cấu hình trong `src/DCRManagement.UI/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=DCRManagement;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "SenderEmail": "your@email.com",
    "SenderName": "DCR System",
    "Password": "your-app-password"
  },
  "Storage": {
    "AttachmentPath": "C:\\DCRAttachments"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

**Connection string alternatives:**
```
# Local SQL Server
Server=localhost;Database=DCRManagement;User Id=sa;Password=YourPassword;TrustServerCertificate=True

# SQL Server Express
Server=.\SQLEXPRESS;Database=DCRManagement;Trusted_Connection=True
```

## EF Core Migration Commands

> Chạy tất cả lệnh từ **root folder** (nơi có `DCRManagement.sln`)

### Tạo migration mới

```bash
dotnet ef migrations add <TênMigration> \
  --project src/DCRManagement.Infrastructure \
  --startup-project src/DCRManagement.UI \
  --output-dir Persistence/Migrations
```

**Ví dụ:**
```bash
dotnet ef migrations add InitialCreate \
  --project src/DCRManagement.Infrastructure \
  --startup-project src/DCRManagement.UI \
  --output-dir Persistence/Migrations
```

### Apply migration lên database

```bash
dotnet ef database update \
  --project src/DCRManagement.Infrastructure \
  --startup-project src/DCRManagement.UI
```

### Apply một migration cụ thể

```bash
dotnet ef database update InitialCreate \
  --project src/DCRManagement.Infrastructure \
  --startup-project src/DCRManagement.UI
```

### Roll back migration

```bash
# Roll back về migration trước đó
dotnet ef database update <TênMigrationCũ> \
  --project src/DCRManagement.Infrastructure \
  --startup-project src/DCRManagement.UI

# Roll back toàn bộ (xóa hết tables)
dotnet ef database update 0 \
  --project src/DCRManagement.Infrastructure \
  --startup-project src/DCRManagement.UI
```

### Xem danh sách migrations

```bash
dotnet ef migrations list \
  --project src/DCRManagement.Infrastructure \
  --startup-project src/DCRManagement.UI
```

### Xóa migration chưa apply

```bash
dotnet ef migrations remove \
  --project src/DCRManagement.Infrastructure \
  --startup-project src/DCRManagement.UI
```

### Generate SQL script (không apply trực tiếp)

```bash
dotnet ef migrations script \
  --project src/DCRManagement.Infrastructure \
  --startup-project src/DCRManagement.UI \
  --output migration.sql
```

## IDesignTimeDbContextFactory (required cho CLI)

Nếu `dotnet ef` báo lỗi không tìm thấy DbContext, tạo file này trong Infrastructure project:

```csharp
// src/DCRManagement.Infrastructure/Persistence/AppDbContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DCRManagement.Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(),
                         "../DCRManagement.UI"))
            .AddJsonFile("appsettings.json")
            .Build();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(config.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
            .Options;

        return new AppDbContext(options);
    }
}
```

## DbSeeder — Seed dữ liệu ban đầu

`DbSeeder` tự động chạy khi app start (cần gọi trong `Program.cs`):

```csharp
// src/DCRManagement.UI/Program.cs (cần cập nhật)
using var scope = serviceProvider.CreateScope();
var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
await seeder.SeedAsync();
```

**Tài khoản được tạo tự động:**

| Username | Password | Role |
|----------|----------|------|
| admin | Admin@123 | Admin |
| engineer01 | Engineer@123 | Engineer |
| reviewer01 | Reviewer@123 | Reviewer |
| approver01 | Approver@123 | Approver |

### Reset database dev (xóa và tạo lại)

```sql
-- Chạy trực tiếp trên SQL Server Management Studio
DROP DATABASE IF EXISTS DCRManagement;
```

Sau đó chạy lại:
```bash
dotnet ef database update --project src/DCRManagement.Infrastructure --startup-project src/DCRManagement.UI
```

## Program.cs — DI Setup hoàn chỉnh

File `Program.cs` hiện tại chỉ có boilerplate. Cần cập nhật thành:

```csharp
using DCRManagement.Application;
using DCRManagement.Infrastructure;
using DCRManagement.Infrastructure.Persistence;
using DCRManagement.UI.Forms;
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

        // Configuration
        var config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        services.AddSingleton<IConfiguration>(config);
        services.AddLogging(b => b.AddConsole().SetMinimumLevel(LogLevel.Information));

        // Infrastructure + Application layers
        services.AddInfrastructure(config);
        services.AddApplication();

        // UI Forms (Transient — new instance each time)
        services.AddTransient<LoginForm>();
        services.AddTransient<MainForm>();
        services.AddTransient<DCRListForm>();
        services.AddTransient<DCRDetailForm>();

        ServiceProvider = services.BuildServiceProvider();

        // Seed database
        using (var scope = ServiceProvider.CreateScope())
        {
            var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
            await seeder.SeedAsync();
        }

        var loginForm = ServiceProvider.GetRequiredService<LoginForm>();
        Application.Run(loginForm);
    }
}
```
