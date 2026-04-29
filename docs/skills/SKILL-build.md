---
name: dcr-build
description: Build và restore project DCRManagement. Dùng khi cần compile code, kiểm tra lỗi build, restore NuGet packages, hoặc chuẩn bị môi trường trước khi chạy tests.
---

# Skill: Build DCRManagement Project

## Điều kiện tiên quyết

- .NET 8 SDK đã cài (`dotnet --version` ≥ 8.0.0)
- File `DCRManagement.sln` tồn tại ở root folder

## Bước 1 — Kiểm tra môi trường

```bash
# Xác nhận .NET version
dotnet --version

# Kiểm tra solution file tồn tại
ls DCRManagement.sln
```

## Bước 2 — Restore NuGet packages

```bash
dotnet restore DCRManagement.sln
```

**Kết quả mong đợi:** `Restore completed` không có lỗi.

**Nếu lỗi:**
- `Unable to find package` → kiểm tra internet connection hoặc NuGet source
- `Project file not found` → xác nhận đang ở đúng thư mục root

## Bước 3 — Build solution

```bash
# Build Debug (mặc định)
dotnet build DCRManagement.sln --configuration Debug --no-restore

# Hoặc build Release
dotnet build DCRManagement.sln --configuration Release --no-restore
```

**Flags hữu ích:**
```bash
--no-restore          # Bỏ qua restore (nếu đã restore rồi)
--verbosity minimal   # Ít output hơn
--verbosity detailed  # Chi tiết hơn khi debug build errors
/p:TreatWarningsAsErrors=false  # Bỏ qua warnings
```

## Bước 4 — Build từng project riêng lẻ (nếu cần isolate lỗi)

```bash
# Build theo thứ tự dependency
dotnet build src/DCRManagement.Domain/DCRManagement.Domain.csproj
dotnet build src/DCRManagement.Infrastructure/DCRManagement.Infrastructure.csproj
dotnet build src/DCRManagement.Application/DCRManagement.Application.csproj
dotnet build src/DCRManagement.UI/DCRManagement.UI.csproj
dotnet build tests/DCRManagement.Tests/DCRManagement.Tests.csproj
```

## Lỗi build thường gặp và cách xử lý

### Missing package references

**Triệu chứng:** `The type or namespace 'BCrypt' could not be found`

**Nguyên nhân:** `Application.csproj` chưa có PackageReference

**Sửa:** Thêm vào `src/DCRManagement.Application/DCRManagement.Application.csproj`:
```xml
<ItemGroup>
  <PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
  <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.0" />
  <ProjectReference Include="..\DCRManagement.Domain\DCRManagement.Domain.csproj" />
</ItemGroup>
```

Thêm vào `src/DCRManagement.UI/DCRManagement.UI.csproj`:
```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
  <PackageReference Include="Microsoft.Extensions.Logging" Version="8.0.0" />
  <PackageReference Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
  <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.0" />
  <ProjectReference Include="..\DCRManagement.Application\DCRManagement.Application.csproj" />
  <ProjectReference Include="..\DCRManagement.Infrastructure\DCRManagement.Infrastructure.csproj" />
</ItemGroup>
```

### WinForms chỉ build được trên Windows

**Triệu chứng:** `DCRManagement.UI` lỗi khi build trên Linux/Mac

**Nguyên nhân:** `<UseWindowsForms>true</UseWindowsForms>` yêu cầu Windows

**Giải pháp trên Linux/CI:** Chỉ build 3 projects không phụ thuộc WinForms:
```bash
dotnet build src/DCRManagement.Domain/DCRManagement.Domain.csproj
dotnet build src/DCRManagement.Infrastructure/DCRManagement.Infrastructure.csproj
dotnet build src/DCRManagement.Application/DCRManagement.Application.csproj
dotnet build tests/DCRManagement.Tests/DCRManagement.Tests.csproj
```

### EF Core Tools không tìm thấy

```bash
dotnet tool install --global dotnet-ef
# hoặc
dotnet tool restore
```

## Output locations

```
src/DCRManagement.Domain/bin/Debug/net8.0/
src/DCRManagement.Infrastructure/bin/Debug/net8.0/
src/DCRManagement.Application/bin/Debug/net8.0/
src/DCRManagement.UI/bin/Debug/net8.0-windows/
tests/DCRManagement.Tests/bin/Debug/net8.0/
```
