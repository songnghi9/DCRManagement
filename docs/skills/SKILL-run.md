---
name: dcr-run
description: Chạy và vận hành ứng dụng DCRManagement WinForms. Dùng khi cần khởi động ứng dụng, kiểm tra luồng login, navigate giữa các form, hoặc debug runtime behavior.
---

# Skill: Chạy DCRManagement

## Prerequisites

- Build thành công (xem SKILL-build.md)
- Database đã migrate và seed (xem SKILL-db.md)
- appsettings.json đã cấu hình
- **Windows OS** (WinForms chỉ chạy trên Windows)

## Chạy ứng dụng

```bash
# Development
dotnet run --project src/DCRManagement.UI

# Hoặc chỉ định configuration
dotnet run --project src/DCRManagement.UI --configuration Debug

# Chạy file exe trực tiếp (sau khi publish)
.\src\DCRManagement.UI\bin\Debug\net8.0-windows\DCRManagement.UI.exe
```

## Publish (Windows executable)

```bash
dotnet publish src/DCRManagement.UI \
  --configuration Release \
  --runtime win-x64 \
  --self-contained true \
  --output ./publish

# Self-contained (~80MB) — không cần .NET Runtime trên máy target
# Hoặc framework-dependent (~5MB) — cần .NET 8 Runtime
dotnet publish src/DCRManagement.UI \
  --configuration Release \
  --runtime win-x64 \
  --self-contained false \
  --output ./publish
```

## Luồng sử dụng cơ bản

### 1. Login
- URL: `LoginForm` (first form)
- Tài khoản dev: `admin` / `Admin@123`
- Sau login → `MainForm` hiển thị

### 2. Tạo DCR mới
```
MainForm → Sidebar "➕ New DCR"
→ DCRDetailForm (Create mode)
→ Điền Title, Description (bắt buộc)
→ Click "💾 Save"
→ DCR-2024-XXXX được tạo
```

### 3. Submit DCR
```
DCRDetailForm (sau khi Save)
→ Click "📤 Submit for Review"
→ SubmitDCRDialog: chọn Reviewer + Approver
→ OK → status chuyển sang "Pending Review"
```

### 4. Review và Approve
```
Login với reviewer01 / Reviewer@123
→ DCR List → tìm DCR được assign
→ View → Click "▶ Start Review" → "Under Review"
→ Click "📨 Send to Approval" → "Pending Approval"

Login với approver01 / Approver@123
→ Click "✅ Approve" → "Approved"
→ Click "🔒 Close DCR" → "Closed"
```

## Form hierarchy

```
Program.Main()
    └── LoginForm
            │ (on success)
            └── MainForm
                    ├── Sidebar navigation
                    ├── DCRListForm (default content)
                    │       └── DCRDetailForm (view/edit)
                    └── UserManagementForm (Phase 6)
```

## Keyboard shortcuts (WinForms defaults)

| Shortcut | Action |
|----------|--------|
| Enter | Trigger AcceptButton (Login button trên LoginForm) |
| Alt+F4 | Đóng form hiện tại |
| Tab | Focus next control |

## Kiểm tra application logs

Logs xuất ra Console khi chạy với `dotnet run`:
```
info: DCRManagement.Application.Services.AuthService[0]
      User admin logged in successfully
info: DCRManagement.Application.Services.WorkflowService[0]
      DCR DCR-2024-0001 transitioned Draft → PendingReview by admin
```

Để xem trong Visual Studio: View → Output → Show output from: Debug

## Smoke test checklist

Chạy qua các bước này để verify app hoạt động:

- [ ] App khởi động không crash
- [ ] Login với `admin/Admin@123` thành công
- [ ] DCR List load (có thể empty)
- [ ] Tạo DCR mới, điền Title + Description, Save
- [ ] DCR number được generate (DCR-YYYY-0001)
- [ ] Submit DCR, chọn reviewer01 và approver01
- [ ] Logout → Login lại với reviewer01
- [ ] Start Review trên DCR vừa tạo
- [ ] Send to Approval
- [ ] Login với approver01, Approve DCR
- [ ] DCR status = Approved
