# Script tạo skeleton cho DCRManagement.sln

# Tạo file solution (nếu bạn đang dùng .NET CLI)
dotnet new sln -n DCRManagement

# Tạo thư mục src và các project
New-Item -ItemType Directory -Path src\DCRManagement.Domain -Force
New-Item -ItemType Directory -Path src\DCRManagement.Infrastructure -Force
New-Item -ItemType Directory -Path src\DCRManagement.Application -Force
New-Item -ItemType Directory -Path src\DCRManagement.UI -Force

# Tạo thư mục tests và project test
New-Item -ItemType Directory -Path tests\DCRManagement.Tests -Force

# Tạo các project dạng class library/WinForms (hoặc bỏ qua nếu chỉ cần thư mục)
dotnet new classlib -n DCRManagement.Domain -o src\DCRManagement.Domain
dotnet new classlib -n DCRManagement.Infrastructure -o src\DCRManagement.Infrastructure
dotnet new classlib -n DCRManagement.Application -o src\DCRManagement.Application
dotnet new winforms -n DCRManagement.UI -o src\DCRManagement.UI
dotnet new xunit -n DCRManagement.Tests -o tests\DCRManagement.Tests

# Thêm các project vào solution
dotnet sln add src\DCRManagement.Domain\DCRManagement.Domain.csproj
dotnet sln add src\DCRManagement.Infrastructure\DCRManagement.Infrastructure.csproj
dotnet sln add src\DCRManagement.Application\DCRManagement.Application.csproj
dotnet sln add src\DCRManagement.UI\DCRManagement.UI.csproj
dotnet sln add tests\DCRManagement.Tests\DCRManagement.Tests.csproj

Write-Host "✅ Skeleton DCRManagement.sln đã được tạo hoàn chỉnh."
``