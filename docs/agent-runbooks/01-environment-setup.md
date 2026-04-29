# Runbook 01 — Environment Setup từ đầu

> Agent chạy runbook này khi cần thiết lập môi trường phát triển hoàn chỉnh từ đầu.

## Checklist

- [ ] .NET 8 SDK cài xong
- [ ] SQL Server / LocalDB ready
- [ ] NuGet packages restored
- [ ] appsettings.json đã cấu hình
- [ ] Database migration applied
- [ ] Seed data chạy xong
- [ ] Build thành công
- [ ] Login form hiển thị

---

## Bước 1 — Kiểm tra prerequisites

```bash
# .NET 8
dotnet --version
# Expected: 8.x.x

# SQL Server LocalDB (Windows)
SqlLocalDB info
# Expected: thấy instance MSSQLLocalDB

# Hoặc SQL Server via Docker (cross-platform)
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd" \
  -p 1433:1433 --name dcr-sqlserver \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

## Bước 2 — Clone / mở project

```bash
# Mở folder chứa DCRManagement.sln
cd <path-to-project>
ls DCRManagement.sln  # phải tồn tại
```

## Bước 3 — Bổ sung PackageReferences còn thiếu

Các file `.csproj` sau cần được cập nhật (xem chi tiết tại `SKILL-build.md`):

### `src/DCRManagement.Application/DCRManagement.Application.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Abstractions" Version="8.0.0" />
    <ProjectReference Include="..\DCRManagement.Domain\DCRManagement.Domain.csproj" />
  </ItemGroup>
</Project>
```

### `src/DCRManagement.UI/DCRManagement.UI.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <Nullable>enable</Nullable>
    <UseWindowsForms>true</UseWindowsForms>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Console" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.0" />
    <ProjectReference Include="..\DCRManagement.Application\DCRManagement.Application.csproj" />
    <ProjectReference Include="..\DCRManagement.Infrastructure\DCRManagement.Infrastructure.csproj" />
  </ItemGroup>
</Project>
```

### `tests/DCRManagement.Tests/DCRManagement.Tests.csproj`
```xml
<ItemGroup>
  <PackageReference Include="Moq" Version="4.20.72" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.0" />
  <PackageReference Include="FluentAssertions" Version="6.12.0" />
  <ProjectReference Include="..\..\src\DCRManagement.Application\DCRManagement.Application.csproj" />
  <ProjectReference Include="..\..\src\DCRManagement.Infrastructure\DCRManagement.Infrastructure.csproj" />
  <ProjectReference Include="..\..\src\DCRManagement.Domain\DCRManagement.Domain.csproj" />
</ItemGroup>
```

## Bước 4 — Tạo appsettings.json

Tạo file `src/DCRManagement.UI/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=DCRManagement;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "SenderEmail": "",
    "SenderName": "DCR System",
    "Password": ""
  },
  "Storage": {
    "AttachmentPath": ""
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

## Bước 5 — Cập nhật Program.cs

Xem nội dung đầy đủ tại `docs/skills/SKILL-db.md` phần "Program.cs — DI Setup hoàn chỉnh".

## Bước 6 — Cài EF Tools và tạo migration

```bash
# Cài EF Tools (nếu chưa có)
dotnet tool install --global dotnet-ef

# Tạo migration đầu tiên
dotnet ef migrations add InitialCreate \
  --project src/DCRManagement.Infrastructure \
  --startup-project src/DCRManagement.UI \
  --output-dir Persistence/Migrations

# Apply lên database
dotnet ef database update \
  --project src/DCRManagement.Infrastructure \
  --startup-project src/DCRManagement.UI
```

## Bước 7 — Build và verify

```bash
dotnet restore DCRManagement.sln
dotnet build DCRManagement.sln --configuration Debug
```

**Expected output:**
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

## Bước 8 — Chạy tests

```bash
dotnet test tests/DCRManagement.Tests/ --logger "console;verbosity=minimal"
```

## Bước 9 — Chạy ứng dụng (Windows only)

```bash
dotnet run --project src/DCRManagement.UI
```

Login với: `admin` / `Admin@123`

---

## Troubleshooting Setup

| Lỗi | Nguyên nhân | Giải pháp |
|-----|-------------|-----------|
| `SDK not found` | .NET 8 chưa cài | Tải từ https://dot.net |
| `LocalDB not found` | SQL Server chưa cài | Cài SQL Server Express hoặc dùng Docker |
| `Cannot open database` | Migration chưa chạy | Chạy `dotnet ef database update` |
| `Form1 not found` | Program.cs chưa update | Xem Bước 5 |
| `ServiceProvider null` | DI chưa setup | Xem SKILL-db.md |
