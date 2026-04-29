# Development Guide — DCRManagement

## 📋 Before You Code

### 1. **Environment Setup**

```bash
# Prerequisites
- .NET 8.0 SDK installed
- SQL Server (LocalDB or full instance)
- Visual Studio 2022 or VS Code + C# extensions

# Clone & restore
git clone <repo>
cd c:\Workbench\AppDev\C_Sharp\DCRManagement
dotnet restore DCRManagement.sln

# Apply migrations
dotnet ef database update --project src/DCRManagement.Infrastructure --startup-project src/DCRManagement.UI

# Seed initial users (if not auto-seeded)
# Default credentials:
# - admin / Admin@123
# - engineer01 / Engineer@123
# - reviewer01 / Reviewer@123
# - approver01 / Approver@123
```

---

## 🏗️ Architecture Quick Reference

### Dependency Flow
```
UI → Application → Domain
UI → Infrastructure → Domain
Infrastructure → Application (for SessionContext)
```

### Layer Responsibilities

| Layer | Responsibility | Examples |
|-------|---|---|
| **Domain** | Entities, interfaces, enums (no external deps) | `DCR.cs`, `IDCRRepository.cs`, `DCRStatus.cs` |
| **Application** | Business logic, services, DTOs | `DCRService.cs`, `DCRDto.cs`, `Result<T>.cs` |
| **Infrastructure** | Data access, external services | `EF Core`, `Repositories`, `EmailService` |
| **UI** | Forms, presenters, user interaction | WinForms, MVP pattern |

---

## 💡 Code Patterns Used

### 1. **Result Pattern** — Error Handling Without Exceptions

```csharp
// ✅ Services return Result<T>, not throwing for business errors
public async Task<Result<DCRDto>> CreateAsync(CreateDCRDto dto)
{
    var validation = ValidateCreateDto(dto);
    if (!validation.IsSuccess)
        return Result<DCRDto>.Failure(validation.ErrorMessage!, validation.ErrorCode);
    
    // ... business logic
    return dcr; // Implicit conversion to Result<DCRDto>.Success(dcr)
}

// ✅ Caller checks result
var result = await _dcrService.CreateAsync(dto);
if (!result.IsSuccess)
{
    MessageBox.Show(result.ErrorMessage);
    return;
}
var dcr = result.Value;
```

**Why:** Exceptions are for truly unexpected errors (DB down, null reference). 
Business logic errors (validation failed, not found) return Result for predictable control flow.

---

### 2. **MVP Pattern** — Separation of Concerns

```csharp
// ✅ View interface
public interface IDCRDetailView
{
    string Title { get; set; }  // ← Presenter reads/writes
    event EventHandler SaveRequested;  // ← Presenter subscribes
    void ShowError(string message);
}

// ✅ WinForms form
public partial class DCRDetailForm : Form, IDCRDetailView
{
    private DCRDetailPresenter _presenter;
    
    public DCRDetailForm(DCRService service, ...)
    {
        InitializeComponent();
        _presenter = new DCRDetailPresenter(this, service, ...);
        
        _btnSave.Click += (s, e) => SaveRequested?.Invoke(this, EventArgs.Empty);
    }
    
    public void ShowError(string message) => MessageBox.Show(message);
}

// ✅ Presenter handles logic
public class DCRDetailPresenter
{
    private readonly IDCRDetailView _view;
    
    public DCRDetailPresenter(IDCRDetailView view, ...)
    {
        _view = view;
        _view.SaveRequested += OnSaveAsync;  // ← Wire events
    }
    
    private async Task OnSaveAsync()
    {
        var dto = new CreateDCRDto(
            _view.Title,      // ← Read from view
            _view.Description,
            ...
        );
        var result = await _dcrService.CreateAsync(dto);
        if (result.IsSuccess)
            _view.ShowSuccess("Saved!");
        else
            _view.ShowError(result.ErrorMessage!);
    }
}
```

**Why:** 
- Form has zero logic (testable without UI)
- Presenter is testable (no UI dependencies)
- Easy to swap implementations (unit test stub vs real form)

---

### 3. **Workflow State Machine** — Centralized Transitions

```csharp
// ✅ Single source of truth for state rules
public class WorkflowService
{
    // Valid transitions: only these combinations allowed
    private static readonly HashSet<(DCRStatus, ApprovalAction)> VALID_TRANSITIONS = new()
    {
        (DCRStatus.Draft, ApprovalAction.Submit),
        (DCRStatus.PendingReview, ApprovalAction.StartReview),
        // ... more rules
    };
    
    // Role-based authorization
    private static readonly Dictionary<ApprovalAction, UserRole[]> ACTION_PERMISSIONS = new()
    {
        { ApprovalAction.Submit, [UserRole.Engineer, UserRole.Admin] },
        { ApprovalAction.Approve, [UserRole.Approver, UserRole.Admin] },
    };
    
    public async Task<Result> ExecuteActionAsync(int dcrId, ApprovalAction action, int actorId, ...)
    {
        // Check permission + transition validity
        var authResult = CheckAuthorization(actor.Role, action, dcr, actorId);
        var transitionResult = CheckTransition(dcr.Status, action);
        
        if (!authResult.IsSuccess || !transitionResult.IsSuccess)
            return failure;
        
        // Execute transition
        dcr.Status = ACTION_RESULT_STATUS[action];
        await _dcrRepository.UpdateAsync(dcr);
        
        // Record history + notify
        await _historyRepository.AddAsync(new ApprovalHistory { ... });
        await NotifyStakeholders(...);
        
        return Result.Success();
    }
}
```

**Why:** No workflow logic scattered across forms or services — all transitions defined here

---

### 4. **Dependency Injection** — Composability

```csharp
// ✅ Program.cs registers all dependencies
services.AddApplication();  // Registers: AuthService, DCRService, UserService, WorkflowService
services.AddInfrastructure(configuration);  // Registers: DbContext, Repositories, EmailService

// ✅ Constructor injection in services
public class DCRService
{
    private readonly IDCRRepository _repo;
    private readonly ILogger<DCRService> _logger;
    
    public DCRService(IDCRRepository repo, ILogger<DCRService> logger)
    {
        _repo = repo;  // ← Injected
        _logger = logger;  // ← Injected
    }
}

// ✅ Easy to mock in tests
var mockRepo = new Mock<IDCRRepository>();
var mockLogger = new Mock<ILogger<DCRService>>();
var service = new DCRService(mockRepo.Object, mockLogger.Object);
```

**Why:** No `new` statements scattered in code → easy to test, swap implementations, manage dependencies

---

### 5. **Global Query Filters** — Soft Delete

```csharp
// ✅ In AppDbContext
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Automatically filter out IsDeleted=true records
    modelBuilder.Entity<DCR>().HasQueryFilter(d => !d.IsDeleted);
    modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
}

// ✅ When you delete
await _dcrRepository.DeleteAsync(dcrId);  // Sets IsDeleted=true, doesn't actually remove

// ✅ Deleted records are invisible everywhere
var dcrs = await _dcrRepository.GetAllAsync();  // Never returns deleted records
```

**Why:** Soft delete preserves audit history, allows un-deletion, simpler than cascading hard deletes

---

## 🛠️ Adding a New Feature

### Step 1: Update Domain Layer (if needed)

```csharp
// src/DCRManagement.Domain/Entities/DCR.cs
public class DCR : BaseEntity
{
    // ✅ Add property
    public string? ApprovalNotes { get; set; }
}
```

### Step 2: Create DTOs

```csharp
// src/DCRManagement.Application/DTOs/DCRDto.cs
public record UpdateApprovalNotesDto(
    int DCRId,
    string ApprovalNotes
);
```

### Step 3: Implement Service Method

```csharp
// src/DCRManagement.Application/Services/DCRService.cs
public async Task<Result> UpdateApprovalNotesAsync(UpdateApprovalNotesDto dto)
{
    var dcr = await _dcrRepository.GetByIdAsync(dto.DCRId);
    if (dcr is null)
        return Result.Failure("DCR not found.", "NOT_FOUND");
    
    // ✅ Authorization check
    if (dcr.CreatedById != SessionContext.Instance.UserId && !SessionContext.Instance.IsAdmin)
        return Result.Failure("Unauthorized", "UNAUTHORIZED");
    
    dcr.ApprovalNotes = dto.ApprovalNotes;
    await _dcrRepository.UpdateAsync(dcr);
    return Result.Success();
}
```

### Step 4: Wire UI (MVP)

```csharp
// src/DCRManagement.UI/Presenters/DCRDetailPresenter.cs
public event EventHandler? UpdateNotesRequested;

public void OnUpdateNotes()
{
    var dto = new UpdateApprovalNotesDto(_view.DCRId, _view.Notes);
    var result = await _dcrService.UpdateApprovalNotesAsync(dto);
    if (result.IsSuccess)
        _view.ShowSuccess("Notes updated");
    else
        _view.ShowError(result.ErrorMessage!);
}
```

### Step 5: Write Tests

```csharp
// tests/DCRManagement.Tests/Services/DCRServiceTests.cs
[Fact]
public async Task UpdateApprovalNotesAsync_WithValidInput_Succeeds()
{
    // Arrange
    var mockRepo = new Mock<IDCRRepository>();
    var dcr = new DCR { Id = 1, CreatedById = 1 };
    mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dcr);
    
    var service = new DCRService(mockRepo.Object, ...);
    SessionContext.Instance.Login(1, "user", "User", "user@test.com", UserRole.Admin);
    
    var dto = new UpdateApprovalNotesDto(1, "New notes");
    
    // Act
    var result = await service.UpdateApprovalNotesAsync(dto);
    
    // Assert
    Assert.True(result.IsSuccess);
    mockRepo.Verify(r => r.UpdateAsync(It.IsAny<DCR>()), Times.Once);
}
```

---

## 🧪 Testing Strategy

### Unit Tests (No External Dependencies)

```csharp
// Test business logic only
// Mock: Repositories, ILogger, IEmailService
[Fact]
public async Task ExecuteAction_WithInvalidTransition_ReturnsFail()
{
    var dcr = new DCR { Status = DCRStatus.Closed };  // Terminal state
    
    var result = await _workflow.ExecuteActionAsync(1, ApprovalAction.Approve, 1);
    
    Assert.False(result.IsSuccess);
    Assert.Equal("INVALID_TRANSITION", result.ErrorCode);
}
```

### Integration Tests (With Real DB)

```csharp
// Test with in-memory SQLite
[Fact]
public async Task CreateDCR_WithValidDto_CreatesRecord()
{
    // Use in-memory SQLite
    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlite("Data Source=:memory:")
        .Options;
    
    using var context = new AppDbContext(options);
    context.Database.EnsureCreated();
    
    var service = new DCRService(new DCRRepository(context), ...);
    var dto = new CreateDCRDto("Title", "Desc", ...);
    
    var result = await service.CreateAsync(dto);
    
    Assert.True(result.IsSuccess);
    Assert.Single(context.DCRs);
}
```

---

## 🐛 Debugging Tips

### 1. Enable SQL Logging

```csharp
// In Program.cs
services.AddLogging(config =>
    config
        .AddConsole()
        .AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Debug)
);
```

### 2. SessionContext is Null

```csharp
// ✅ Always check before accessing
if (SessionContext.Instance.IsAuthenticated)
{
    var userId = SessionContext.Instance.UserId;
}
else
{
    MessageBox.Show("Please login first");
}
```

### 3. Email Not Sending?

```csharp
// Check appsettings.json:
// - SmtpHost, SmtpPort, SenderEmail, Password must be correct
// - For Gmail: use App Password, not account password
// - Enable "Less secure app access" or use 2FA app password
```

---

## 📚 References

- [Architecture Overview](architecture/system-overview.md)
- [State Machine Rules](architecture/state-machine.md)
- [Build & Run](agent-runbooks/02-build-and-verify.md)
- [Testing Guide](test-plans/unit-tests.md)
