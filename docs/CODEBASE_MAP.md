# Codebase Map — DCRManagement

Quick reference để tìm kiếm các class, interface, file quan trọng.

---

## 📂 Project Structure

```
DCRManagement/
├── src/
│   ├── DCRManagement.Domain/
│   ├── DCRManagement.Application/
│   ├── DCRManagement.Infrastructure/
│   └── DCRManagement.UI/
├── tests/
│   └── DCRManagement.Tests/
├── docs/
└── DCRManagement.sln
```

---

## 🔍 Finding Things

### Entities (Domain Models)

| File | Purpose | Key Properties |
|------|---------|---|
| `Domain/Entities/DCR.cs` | Document Change Request | `DCRNumber`, `Status`, `Title`, `AssignedReviewerId`, `AssignedApproverId` |
| `Domain/Entities/User.cs` | System User | `Username`, `PasswordHash`, `Role`, `Department` |
| `Domain/Entities/ApprovalHistory.cs` | Workflow History | `DCRId`, `ActorId`, `Action`, `FromStatus`, `ToStatus` |
| `Domain/Entities/Attachment.cs` | DCR Attachments | `DCRId`, `FileName`, `FilePath`, `FileSizeBytes` |

### Enums (Constants)

| File | Values |
|------|--------|
| `Domain/Enums/DCRStatus.cs` | Draft, PendingReview, UnderReview, PendingApproval, Approved, Rejected, Closed, Cancelled |
| `Domain/Enums/ApprovalAction.cs` | Submit, StartReview, SendToApproval, Approve, Reject, Close, Cancel |
| `Domain/Enums/UserRole.cs` | Admin, Engineer, Reviewer, Approver |

### Interfaces (Contracts)

| File | Purpose | Key Methods |
|------|---------|---|
| `Domain/Interfaces/IRepository.cs` | Generic CRUD | `GetByIdAsync`, `GetAllAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync` |
| `Domain/Interfaces/IDCRRepository.cs` | DCR-specific queries | `GetByStatusAsync`, `GetByCreatorAsync`, `GetWithFullDetailsAsync`, `GenerateNextDCRNumberAsync` |
| `Domain/Interfaces/IUserRepository.cs` | User queries | `GetByUsernameAsync`, `GetByRoleAsync`, `ExistsAsync` |
| `Domain/Interfaces/IEmailService.cs` | Email notifications | `SendAsync`, `SendDCRStatusChangedAsync` |

### DTOs (Data Transfer Objects)

| File | Purpose |
|------|---------|
| `Application/DTOs/DCRDto.cs` | Full DCR details + `CreateDCRDto`, `UpdateDCRDto`, `SubmitDCRDto` |
| `Application/DTOs/UserDto.cs` | User info + `CreateUserDto`, `UpdateUserDto`, `UserSummaryDto` |
| `Application/DTOs/ApprovalHistoryDto.cs` | History entry display |
| `Application/DTOs/AttachmentDto.cs` | Attachment metadata |

### Services (Business Logic)

| File | Responsibility | Key Methods |
|------|---|---|
| `Application/Services/AuthService.cs` | Login/Logout | `LoginAsync`, `Logout` |
| `Application/Services/DCRService.cs` | DCR CRUD | `CreateAsync`, `UpdateAsync`, `SubmitAsync`, `GetAsync`, `GetAllAsync` |
| `Application/Services/UserService.cs` | User CRUD | `CreateUserAsync`, `UpdateUserAsync`, `GetActiveUsersAsync` |
| `Application/Services/WorkflowService.cs` | Approval Workflow | `ExecuteActionAsync` (state machine logic) |
| `Infrastructure/Services/EmailService.cs` | Email Delivery | `SendAsync`, `SendDCRStatusChangedAsync` |
| `Infrastructure/Services/AttachmentService.cs` | File Management | `SaveAttachmentAsync`, `GetAttachmentAsync`, `DeleteAttachmentAsync` |

### Repositories (Data Access)

| File | Purpose | Queries |
|------|---------|---------|
| `Infrastructure/Persistence/Repositories/GenericRepository.cs` | Base CRUD ops | `GetByIdAsync`, `FindAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync` |
| `Infrastructure/Persistence/Repositories/DCRRepository.cs` | DCR queries | `GetByStatusAsync`, `GetAssignedToUserAsync`, `GenerateNextDCRNumberAsync` |
| `Infrastructure/Persistence/Repositories/UserRepository.cs` | User queries | `GetByUsernameAsync`, `GetByRoleAsync` |
| `Infrastructure/Persistence/Repositories/ApprovalHistoryRepository.cs` | History queries | `GetByDCRIdAsync` |

### UI Forms

| File | Purpose | Implements |
|------|---------|---|
| `UI/Forms/LoginForm.cs` | User Authentication | `ILoginView` |
| `UI/Forms/MainForm.cs` | Main Dashboard | `IMainView` |
| `UI/Forms/DCRListForm.cs` | List of DCRs | `IDCRListView` |
| `UI/Forms/DCRDetailForm.cs` | Create/Edit/View DCR | `IDCRDetailView` |

### Presenters (MVP Middle Layer)

| File | Coordinates | Interfaces |
|------|---|---|
| `UI/Presenters/LoginPresenter.cs` | Login flow | `ILoginView` |
| `UI/Presenters/MainPresenter.cs` | Main dashboard | `IMainView` |
| `UI/Presenters/DCRListPresenter.cs` | List operations | `IDCRListView` |
| `UI/Presenters/DCRDetailPresenter.cs` | Create/Edit/View | `IDCRDetailView` |

### Core Infrastructure

| File | Purpose |
|------|---------|
| `Application/Common/Result.cs` | Error handling pattern (Result<T>) |
| `Application/Common/SessionContext.cs` | Current user singleton |
| `Infrastructure/Persistence/AppDbContext.cs` | EF Core DbContext (soft-delete filters) |
| `Infrastructure/Persistence/DbSeeder.cs` | Initial data seeding |

### Configuration

| File | Purpose |
|------|---------|
| `UI/appsettings.json` | DB connection, logging, email settings |
| `Application/DependencyInjection.cs` | Service registrations |
| `Infrastructure/DependencyInjection.cs` | EF Core, repo registrations |

---

## 🔗 Common Queries

### "How do I add a new DCR field?"

1. Add property to `Domain/Entities/DCR.cs`
2. Add to `Application/DTOs/DCRDto.cs` record definition
3. Add to `CreateDCRDto` and `UpdateDCRDto` if needed
4. Update `DCRService.MapToDtoAsync()` mapping
5. Create EF Core migration: `dotnet ef migrations add AddDCRField`
6. Update UI forms (`DCRDetailForm`) to show field
7. Write tests in `tests/DCRManagement.Tests/`

### "How do I add a new approval action?"

1. Add to `Domain/Enums/ApprovalAction.cs`
2. Update `WorkflowService.VALID_TRANSITIONS` with new rules
3. Update `WorkflowService.ACTION_PERMISSIONS` with role access
4. Update `WorkflowService.ACTION_RESULT_STATUS` with resulting DCRStatus
5. Update `ApprovalHistoryDto.ActionDisplay` switch statement
6. Test in `tests/DCRManagement.Tests/Services/WorkflowServiceTests.cs`

### "Where's the state machine logic?"

→ `Application/Services/WorkflowService.cs` — all transition rules in dictionaries at class top

### "Where's the email configuration?"

→ `UI/appsettings.json` under `Email` section  
→ Implementation: `Infrastructure/Services/EmailService.cs`

### "Where's the current logged-in user?"

→ `Application/Common/SessionContext.Instance`  
→ Set during login in `AuthService.LoginAsync()`  
→ Cleared during logout in `AuthService.Logout()`

### "How do I get all DCRs?"

```csharp
var dcrs = await _dcrRepository.GetAllAsync();  // Generic method

// Or specialized:
var myDcrs = await _dcrRepository.GetByCreatorAsync(userId);
var assigned = await _dcrRepository.GetAssignedToUserAsync(userId);
var pending = await _dcrRepository.GetByStatusAsync(DCRStatus.PendingReview);

// Or with full details:
var detailed = await _dcrRepository.GetWithFullDetailsAsync(dcrId);
```

### "How do I execute a workflow action?"

```csharp
var result = await _workflowService.ExecuteActionAsync(
    dcrId: 5,
    action: ApprovalAction.Approve,
    actorId: SessionContext.Instance.UserId,
    comment: "Looks good"
);

if (result.IsSuccess)
    MessageBox.Show("Approved!");
else
    MessageBox.Show(result.ErrorMessage);
```

---

## 📊 Class Diagram (Simplified)

```
┌──────────────────────────────────────────────────────────┐
│                      DCR (Entity)                         │
│  ─ Id: int                                                │
│  ─ DCRNumber: string (auto-generated)                     │
│  ─ Title, Description, Status                            │
│  ─ AssignedReviewerId, AssignedApproverId                │
│  ─ CreatedById, UpdatedById                              │
│  ─ IsDeleted (soft-delete)                               │
│  ─ Nav: AssignedReviewer, AssignedApprover              │
│  ─ Nav: ApprovalHistories, Attachments                  │
└──────────────────────────────────────────────────────────┘
         ▲
         │ Dto
         │
┌──────────────────────────────────────────────────────────┐
│                   DCRDto (DTO)                            │
│  + StatusDisplay { Draft → "Draft", ... }               │
│  + IsEditable { Draft or Rejected only }                │
│  + IsClosed { Closed or Cancelled }                     │
└──────────────────────────────────────────────────────────┘
         ▲
         │ mapped by
         │
┌──────────────────────────────────────────────────────────┐
│                  DCRService                              │
│  ─ _dcrRepository: IDCRRepository                        │
│  ─ _workflowService: WorkflowService                     │
│  ─ _logger: ILogger<DCRService>                          │
│  + CreateAsync(CreateDCRDto): Task<Result<DCRDto>>      │
│  + UpdateAsync(UpdateDCRDto): Task<Result<DCRDto>>      │
│  + SubmitAsync(SubmitDCRDto): Task<Result>              │
└──────────────────────────────────────────────────────────┘
         ▲
         │ used by
         │
┌──────────────────────────────────────────────────────────┐
│              DCRDetailPresenter                          │
│  ─ _dcrService: DCRService                              │
│  ─ _workflowService: WorkflowService                     │
│  + OnSaveAsync()                                        │
│  + OnSubmitAsync()                                      │
│  + OnWorkflowActionAsync(ApprovalAction)               │
└──────────────────────────────────────────────────────────┘
         ▲
         │ wires
         │
┌──────────────────────────────────────────────────────────┐
│            DCRDetailForm (WinForms)                       │
│  Implements: IDCRDetailView                              │
│  ─ _txtTitle: TextBox                                    │
│  ─ _cmbStatus: ComboBox                                  │
│  ─ _btnSave: Button                                      │
│  Event: SaveRequested                                    │
│  Event: WorkflowActionRequested                         │
└──────────────────────────────────────────────────────────┘
```

---

## 🚀 Common Tasks

### Build the solution
```bash
cd c:\Workbench\AppDev\C_Sharp\DCRManagement
dotnet build DCRManagement.sln
```

### Run migrations
```bash
dotnet ef database update --project src/DCRManagement.Infrastructure --startup-project src/DCRManagement.UI
```

### Run tests
```bash
dotnet test tests/DCRManagement.Tests/DCRManagement.Tests.csproj --logger "console;verbosity=detailed"
```

### Run the app
```bash
dotnet run --project src/DCRManagement.UI/DCRManagement.UI.csproj
```

---

## 📖 Related Documentation

- [Architecture Overview](architecture/system-overview.md) — Layer breakdown
- [State Machine Rules](architecture/state-machine.md) — Workflow diagram
- [Changes Log](CHANGES.md) — Recent fixes
- [Development Guide](DEVELOPMENT_GUIDE.md) — Patterns & examples
- [Refactoring Opportunities](REFACTORING_OPPORTUNITIES.md) — Future improvements
