# System Overview — DCRManagement

## Kiến trúc tổng quan

Hệ thống theo **Clean Architecture** với 4 layers:

```
┌─────────────────────────────────────────┐
│         DCRManagement.UI                │  WinForms, Presenters, Forms
│    (net8.0-windows, UseWindowsForms)    │
└────────────────┬────────────────────────┘
                 │ depends on
┌────────────────▼────────────────────────┐
│       DCRManagement.Application         │  Services, DTOs, Result<T>
│             (net8.0)                    │
└────────────┬───────────┬────────────────┘
             │           │ depends on
┌────────────▼──┐   ┌────▼───────────────┐
│    Domain     │   │  Infrastructure    │
│  Entities,    │   │  EF Core, Repos,   │
│  Interfaces,  │   │  Email, Attachments│
│  Enums        │   │  (net8.0)          │
└───────────────┘   └────────────────────┘
```

## Projects & Dependencies

| Project | Framework | Tác dụng |
|---------|-----------|----------|
| `DCRManagement.Domain` | net8.0 | Entities, Interfaces, Enums — zero dependencies |
| `DCRManagement.Infrastructure` | net8.0 | EF Core SqlServer, MailKit, BCrypt |
| `DCRManagement.Application` | net8.0 | Services, DTOs, SessionContext, Result |
| `DCRManagement.UI` | net8.0-windows | WinForms, Presenters, Forms |
| `DCRManagement.Tests` | net8.0 | xUnit test project |

## Dependency Graph

```
UI → Application → Domain
UI → Infrastructure → Domain
Infrastructure → Application (for SessionContext)
```

## Key NuGet Packages

```xml
<!-- Infrastructure -->
Microsoft.EntityFrameworkCore.SqlServer  8.0.0
Microsoft.EntityFrameworkCore.Tools      8.0.0
MailKit                                  4.16.0
BCrypt.Net-Next                          4.0.3
Microsoft.Extensions.Logging.Abstractions 8.0.0
```

> ⚠️ Application và UI `.csproj` hiện chưa khai báo `<PackageReference>` — cần bổ sung BCrypt, EF, Logging khi build.

## Database Schema (SQL Server)

```sql
Users           -- Id, Username, PasswordHash, FullName, Email, Role, Department, IsActive, ...BaseEntity
DCRs            -- Id, DCRNumber (unique), Title, Description, Status (string), AssignedReviewerId?, AssignedApproverId?, ...
ApprovalHistories -- Id, DCRId, ActorId, Action (string), FromStatus, ToStatus, Comment, ActionDate
Attachments     -- Id, DCRId, FileName, StoredFileName, FilePath, FileSizeBytes, ContentType
```

### Soft Delete
`AppDbContext` áp dụng **global query filter** cho `DCR`, `User`, `Attachment`:
```csharp
modelBuilder.Entity<DCR>().HasQueryFilter(d => !d.IsDeleted);
```
Mọi query tự động lọc bỏ các record đã xóa mềm.

## Data Flow — Tạo và Submit DCR

```
User fills form
    │
    ▼
DCRDetailForm (View)
    │  raises SaveRequested event
    ▼
DCRDetailPresenter.OnSaveAsync()
    │  calls
    ▼
DCRService.CreateAsync(CreateDCRDto)
    │  validates → generates DCRNumber → saves
    ▼
IDCRRepository.AddAsync(dcr)
    │
    ▼ returns Result<DCRDto>
DCRDetailPresenter updates View
    │
    ▼ (after save) user clicks Submit
DCRDetailPresenter.OnSubmitAsync()
    │  opens SubmitDCRDialog → picks Reviewer + Approver
    ▼
DCRService.SubmitAsync(SubmitDCRDto)
    │  validates → assigns reviewer/approver
    ▼
WorkflowService.ExecuteActionAsync(Submit)
    │  checks auth → checks transition → updates status
    │  → records ApprovalHistory → fires email notifications
    ▼
Result.Success()
```

## Pattern: MVP (Model-View-Presenter)

```
Form (View)           Presenter              Service
──────────            ─────────              ───────
Implements IView  →   Holds IView ref    →   Business logic
Raises events     ←   Subscribes events  ←   Returns Result<T>
Binds data        ←   Calls View methods
```

**View không biết gì về business logic.** Mọi decision đều ở Presenter và Service.

## SessionContext

Singleton lưu thông tin user đang đăng nhập:
```csharp
SessionContext.Instance.UserId       // int
SessionContext.Instance.Role         // UserRole enum
SessionContext.Instance.IsAdmin      // bool
SessionContext.Instance.CanCreateDCR // Admin or Engineer
SessionContext.Instance.CanReview    // Admin or Reviewer
SessionContext.Instance.CanApprove   // Admin or Approver
```

## Result Pattern

```csharp
// Service trả về Result<T> thay vì throw exception
Result<DCRDto> result = await _dcrService.CreateAsync(dto);

if (!result.IsSuccess)
{
    _view.ShowError(result.ErrorMessage!, "Error");
    return;
}
var dcr = result.Value!;
```
