# Runbook 02 — Build và Verify

> Agent chạy runbook này để verify project build sạch sau mỗi thay đổi code.

## Khi nào chạy

- Sau khi thay đổi `.csproj`
- Sau khi thêm file mới
- Trước khi commit
- Sau khi merge code

## Script đầy đủ

```bash
#!/bin/bash
# verify-build.sh — chạy từ root folder

echo "=== Step 1: Restore packages ==="
dotnet restore DCRManagement.sln
if [ $? -ne 0 ]; then echo "❌ Restore FAILED"; exit 1; fi

echo "=== Step 2: Build Domain ==="
dotnet build src/DCRManagement.Domain/DCRManagement.Domain.csproj --no-restore --configuration Debug
if [ $? -ne 0 ]; then echo "❌ Domain build FAILED"; exit 1; fi

echo "=== Step 3: Build Infrastructure ==="
dotnet build src/DCRManagement.Infrastructure/DCRManagement.Infrastructure.csproj --no-restore --configuration Debug
if [ $? -ne 0 ]; then echo "❌ Infrastructure build FAILED"; exit 1; fi

echo "=== Step 4: Build Application ==="
dotnet build src/DCRManagement.Application/DCRManagement.Application.csproj --no-restore --configuration Debug
if [ $? -ne 0 ]; then echo "❌ Application build FAILED"; exit 1; fi

echo "=== Step 5: Build Tests ==="
dotnet build tests/DCRManagement.Tests/DCRManagement.Tests.csproj --no-restore --configuration Debug
if [ $? -ne 0 ]; then echo "❌ Tests build FAILED"; exit 1; fi

echo "=== Step 6: Run Tests ==="
dotnet test tests/DCRManagement.Tests/ --no-build --logger "console;verbosity=minimal"
if [ $? -ne 0 ]; then echo "❌ Tests FAILED"; exit 1; fi

echo ""
echo "✅ All checks passed!"
```

> **Note:** Bỏ qua build `DCRManagement.UI` trên Linux/CI vì yêu cầu Windows (UseWindowsForms).

## Kết quả mong đợi

```
=== Step 1: Restore packages ===
  Restore completed (3.2s)

=== Step 2: Build Domain ===
  Build succeeded. 0 Warning(s) 0 Error(s)

=== Step 3: Build Infrastructure ===
  Build succeeded. 0 Warning(s) 0 Error(s)

=== Step 4: Build Application ===
  Build succeeded. 0 Warning(s) 0 Error(s)

=== Step 5: Build Tests ===
  Build succeeded. 0 Warning(s) 0 Error(s)

=== Step 6: Run Tests ===
  Passed! - Failed: 0, Passed: 38, Skipped: 0, Total: 38

✅ All checks passed!
```

## Verify không có circular dependencies

```bash
# Kiểm tra Domain không reference bất kỳ layer nào khác
grep -r "ProjectReference" src/DCRManagement.Domain/
# Expected: không có output (Domain không có ProjectReference)

# Infrastructure chỉ reference Domain
grep -r "ProjectReference" src/DCRManagement.Infrastructure/
# Expected: chỉ thấy DCRManagement.Domain.csproj
```

## Verify không có hardcoded connection strings

```bash
grep -r "Server=" src/ --include="*.cs"
# Expected: không có output (connection string chỉ trong appsettings.json)
```
