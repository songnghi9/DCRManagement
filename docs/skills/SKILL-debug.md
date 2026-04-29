---
name: dcr-debug
description: Debug các lỗi thường gặp trong DCRManagement — runtime errors, EF Core issues, WinForms threading, DI problems. Dùng khi gặp exception, UI không phản hồi, data không load, hoặc email không gửi được.
---

# Skill: Debug DCRManagement

## Lỗi thường gặp và cách xử lý

---

### 1. `NullReferenceException` trong Presenter

**Triệu chứng:** Crash khi gọi `_view.ShowError(...)` hoặc `_currentDcr.Id`

**Nguyên nhân phổ biến:**
- `_currentDcr` chưa được load trước khi dùng
- View chưa được khởi tạo đúng

**Debug:**
```csharp
// Kiểm tra null trước khi dùng
if (_currentDcr is null)
{
    _view.ShowError("DCR chưa được load.", "Error");
    return;
}
```

---

### 2. `InvalidOperationException: Cross-thread operation`

**Triệu chứng:** Lỗi khi update UI từ async/background thread

**Nguyên nhân:** WinForms controls phải được update trên UI thread

**Sửa — dùng `InvokeIfRequired` từ BaseForm:**
```csharp
// BaseForm cần có method này:
protected void InvokeIfRequired(Action action)
{
    if (InvokeRequired)
        Invoke(action);
    else
        action();
}

// Presenter gọi qua View:
_view.SetHeaderInfo(...) → bên trong Form: InvokeIfRequired(() => { ... })
```

---

### 3. `DbUpdateException` — constraint violation

**Triệu chứng:** Lỗi khi save entity có foreign key sai

**Debug SQL:**
```csharp
// Trong AppDbContext, bật logging chi tiết:
optionsBuilder.UseSqlServer(conn)
    .LogTo(Console.WriteLine, LogLevel.Information)
    .EnableSensitiveDataLogging(); // CHỈ DEV
```

**Kiểm tra:**
- `AssignedReviewerId` phải là ID của user có `Role = Reviewer`
- `CreatedById` phải tồn tại trong bảng Users
- `DCRNumber` phải unique

---

### 4. EF Core: `Sequence contains no elements`

**Triệu chứng:** `InvalidOperationException` khi gọi `.First()` trên empty collection

**Sửa:** Dùng `.FirstOrDefault()` hoặc `.FirstOrDefaultAsync()` thay vì `.First()`

```csharp
// BAD
var dcr = await _dbSet.Where(...).First();

// GOOD
var dcr = await _dbSet.Where(...).FirstOrDefaultAsync();
if (dcr is null) return Result.Failure("Not found", "NOT_FOUND");
```

---

### 5. Lazy loading navigation properties là null

**Triệu chứng:** `dcr.AssignedReviewer` là null dù đã set `AssignedReviewerId`

**Nguyên nhân:** EF Core không lazy load theo mặc định; phải dùng `.Include()`

**Sửa — dùng `GetWithFullDetailsAsync` thay vì `GetByIdAsync`:**
```csharp
// GetByIdAsync → chỉ load DCR, navigation props = null
// GetWithFullDetailsAsync → load đầy đủ với Include
var dcr = await _dcrRepository.GetWithFullDetailsAsync(dcrId);
```

**Hoặc thêm Include trong query:**
```csharp
await _dbSet
    .Include(d => d.AssignedReviewer)
    .Include(d => d.AssignedApprover)
    .FirstOrDefaultAsync(d => d.Id == id);
```

---

### 6. `ObjectDisposedException` — DbContext đã dispose

**Triệu chứng:** Lỗi sau khi scope kết thúc

**Nguyên nhân:** Dùng `_dcrRepository` sau khi DI scope đã dispose

**Sửa:** Đảm bảo services là `Scoped`, không dùng repository trong callback async dài:
```csharp
// BAD — capture repository trong lambda long-running
var data = await Task.Run(() => _repo.GetAllAsync()); // repo có thể đã dispose

// GOOD — await trực tiếp
var data = await _repo.GetAllAsync();
```

---

### 7. Email không gửi được

**Triệu chứng:** Không có email nhưng workflow vẫn thành công (đúng hành vi)

**Debug — kiểm tra logs:**
```
[Warning] Notification failed for DCR DCR-2024-0001 — workflow unaffected
```

**Kiểm tra:**
1. `appsettings.json` có đúng SMTP config không?
2. Gmail: bật "App Passwords" hoặc "Less secure apps"
3. Port 587 với StartTLS, hoặc 465 với SSL:
```csharp
// EmailService.cs — thay đổi SecureSocketOptions nếu cần
await client.ConnectAsync(SmtpHost, SmtpPort, MailKit.Security.SecureSocketOptions.Auto);
```

---

### 8. `DCR-YYYY-NNNN` trùng số (race condition)

**Triệu chứng:** Hai DCR có cùng number khi tạo đồng thời

**Nguyên nhân:** `GenerateNextDCRNumberAsync` dùng MAX query, không atomic

**Sửa tạm (dev):** Thêm unique index (đã có) để SQL Server báo lỗi rõ ràng.

**Sửa production:** Dùng sequence SQL Server:
```sql
CREATE SEQUENCE DCRSequence START WITH 1 INCREMENT BY 1;
```
```csharp
// Trong DCRRepository:
var seq = await _context.Database.ExecuteSqlRawAsync("SELECT NEXT VALUE FOR DCRSequence");
```

---

### 9. WinForms DataGridView không refresh

**Triệu chứng:** Grid vẫn hiện dữ liệu cũ sau khi update

**Sửa:**
```csharp
// Cần reassign DataSource để trigger refresh
_grid.DataSource = null;
_grid.DataSource = newList;

// Hoặc BindingList với notification:
var bindingList = new BindingList<DCRDto>(dcrs.ToList());
_grid.DataSource = bindingList;
```

---

### 10. DI: `ServiceProvider` chưa được set

**Triệu chứng:** `Program.ServiceProvider` là null khi form cố resolve service

**Nguyên nhân:** `Program.cs` chưa được cập nhật (đang dùng boilerplate `Application.Run(new Form1())`)

**Sửa:** Xem `SKILL-db.md` phần "Program.cs — DI Setup hoàn chỉnh"

---

## Logging setup

Thêm logging để debug dễ hơn:

```csharp
// Program.cs
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.AddDebug(); // Output ra VS Debug window
    builder.SetMinimumLevel(LogLevel.Debug); // DEV only
});
```

**Xem logs trong Visual Studio:** View → Output → chọn "Debug"

---

## Kiểm tra nhanh khi có lỗi bí

```bash
# 1. Clean và rebuild
dotnet clean DCRManagement.sln
dotnet build DCRManagement.sln

# 2. Kiểm tra database state
dotnet ef migrations list --project src/DCRManagement.Infrastructure --startup-project src/DCRManagement.UI

# 3. Xem pending changes của EF
dotnet ef migrations has-pending-model-changes --project src/DCRManagement.Infrastructure --startup-project src/DCRManagement.UI
```
