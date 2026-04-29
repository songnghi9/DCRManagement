# Code Standards & Conventions — DCRManagement

> Các quy tắc & conventions tất cả developers phải tuân theo để giữ codebase consistent và maintainable.

---

## 🎯 Core Principles

1. **Clean Architecture** — Domain ← Application ← Infrastructure ← UI (one-way dependency flow)
2. **Result Pattern** — Never throw for business logic errors, always return `Result<T>`
3. **MVP Pattern** — Forms have zero logic, all logic in Presenters
4. **Single Responsibility** — Each class does one thing well
5. **Dependency Injection** — No `new` keyword except in DI setup

---

## 📋 Naming Conventions

### Classes & Records

```csharp
// ✅ Services
public class DCRService { }          // Suffixed with "Service"
public class AuthenticationService { }

// ✅ Repositories
public class DCRRepository { }       // Suffixed with "Repository"
public class UserRepository { }

// ✅ Presenters
public class DCRListPresenter { }    // Suffixed with "Presenter"

// ✅ DTOs (Data Transfer Objects)
public record DCRDto { }             // Suffixed with "Dto"
public record CreateDCRDto { }       // "Create" prefix for insert operations
public record UpdateDCRDto { }       // "Update" prefix for updates

// ✅ View Interfaces
public interface IDCRDetailView { }  // Prefixed with "I", suffixed with "View"

// ✅ Forms
public partial class DCRDetailForm { }  // Suffixed with "Form"
```

### Methods

```csharp
// ✅ Async methods — always end with "Async"
public async Task<DCRDto> GetDCRAsync(int id) { }
public async Task CreateDCRAsync(CreateDCRDto dto) { }

// ✅ Sync methods — no "Async" suffix
public string FormatDCRNumber(int sequence) { }

// ✅ Event handlers
private void OnSaveClicked(object sender, EventArgs e) { }
private async Task OnSubmitAsync() { }

// ✅ Validation methods
private Result ValidateCreateDto(CreateDCRDto dto) { }
private async Task<bool> IsUserAuthorizedAsync(int userId) { }
```

### Properties

```csharp
// ✅ Public properties — PascalCase
public string Title { get; set; }
public DateTime CreatedAt { get; set; }

// ✅ Private fields — _camelCase with underscore
private readonly IDCRRepository _dcrRepository;
private int _currentDcrId;

// ✅ Constants — ALL_CAPS_WITH_UNDERSCORES
private const string DCR_NUMBER_PREFIX = "DCR-";
private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromHours(1);
```

### Variables

```csharp
public async Task<Result> ExecuteAsync(SubmitDCRDto dto)
{
    var dcr = await _dcrRepository.GetByIdAsync(dto.DCRId);  // ✅ Local: camelCase
    if (dcr is null)
        return Result.Failure("Not found");
    
    return Result.Success();
}
```

---

## 🏗️ File Organization

### Project Structure

```
DCRManagement.Application/
├── Common/
│   ├── Result.cs              // Generic error/success result
│   ├── SessionContext.cs      // Current user singleton
│   └── PagedResult.cs         // Paging helper
├── DTOs/
│   ├── DCRDto.cs              // All DCR DTOs (Create, Update, etc.)
│   ├── UserDto.cs
│   ├── ApprovalHistoryDto.cs
│   └── AttachmentDto.cs
├── Services/
│   ├── AuthService.cs         // Login/Logout
│   ├── DCRService.cs          // DCR CRUD
│   ├── UserService.cs         // User CRUD
│   └── WorkflowService.cs     // State machine logic
├── DependencyInjection.cs    // Service registrations
└── DCRManagement.Application.csproj
```

**One class per file** (except closely related DTOs in same file)

### Organizing DTOs

```csharp
// ✅ One file per domain entity
// File: DTOs/DCRDto.cs

using DCRManagement.Domain.Enums;

namespace DCRManagement.Application.DTOs;

// Display model
public record DCRDto(
    int Id,
    string DCRNumber,
    string Title,
    ...
)
{
    public bool IsEditable => Status is DCRStatus.Draft or DCRStatus.Rejected;
}

// Input models
public record CreateDCRDto(string Title, string Description, ...);
public record UpdateDCRDto(int Id, string Title, ...);
public record SubmitDCRDto(int DCRId, int AssignedReviewerId, ...);
```

---

## 🔴 Code Style Rules

### C# Language Features

```csharp
// ✅ Use records for immutable DTOs
public record DCRDto(int Id, string Title);

// ✅ Use nullable reference types (enabled in .csproj)
public string? Description { get; set; }  // Optional
public string Title { get; set; } = string.Empty;  // Required

// ✅ Use null-conditional & null-coalescing
var reviewer = dcr.AssignedReviewer;      // ✅ May be null
var status = dcr?.Status ?? DCRStatus.Draft;  // ✅ Safe

// ✅ Use string interpolation for simplicity (except logging)
var message = $"Created DCR {dcr.DCRNumber}";  // ✅

// ✅ Use switch expressions for mapping
public string GetStatusDisplay(DCRStatus status) => status switch
{
    DCRStatus.Draft => "Draft",
    DCRStatus.PendingReview => "Pending Review",
    _ => status.ToString()
};

// ✅ Use LINQ method syntax (readable in chains)
var results = users
    .Where(u => u.Role == UserRole.Reviewer)
    .OrderBy(u => u.FullName)
    .Select(u => new UserSummaryDto(u.Id, u.FullName, u.Email, u.Role))
    .ToList();
```

### Error Handling

```csharp
// ❌ NEVER: throw for business logic
public async Task<DCRDto> GetAsync(int id)
{
    var dcr = await _repo.GetByIdAsync(id);
    throw new KeyNotFoundException("Not found");  // ❌ WRONG
}

// ✅ CORRECT: return Result for business errors
public async Task<Result<DCRDto>> GetAsync(int id)
{
    var dcr = await _repo.GetByIdAsync(id);
    if (dcr is null)
        return Result<DCRDto>.Failure("DCR not found", "NOT_FOUND");  // ✅
    
    return dcr;  // Implicit conversion to Result<DCRDto>.Success(dcr)
}

// ✅ ONLY throw for truly unexpected errors (infrastructure/bugs)
public async Task ProcessAsync()
{
    try
    {
        // ... business logic
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error in ProcessAsync");
        throw;  // Re-throw infrastructure errors
    }
}
```

### Logging

```csharp
// ❌ String interpolation in logs (loses structure)
_logger.LogInformation($"User {username} created DCR {dcrNumber}");

// ✅ Structured logging with positional params
_logger.LogInformation("User {Username} created DCR {DCRNumber}", username, dcrNumber);

// Log levels:
_logger.LogDebug("Detailed info for debugging");           // Development
_logger.LogInformation("Important business event");        // Info level
_logger.LogWarning("Recoverable issue (email failed)");    // Warning
_logger.LogError(ex, "Unrecoverable error");              // Error
_logger.LogCritical(ex, "System cannot continue");        // Critical
```

### Async/Await

```csharp
// ✅ Always use async
public async Task<DCRDto> GetAsync(int id)
{
    return await _repo.GetByIdAsync(id);
}

// ❌ NEVER use Task.Result or .Wait()
public DCRDto Get(int id)
{
    return _repo.GetByIdAsync(id).Result;  // ❌ DEADLOCK RISK!
}

// ✅ Always await, never fire-and-forget unless intentional
_ = EmailService.SendAsync(...);  // ✅ Intentional fire-and-forget (comment why)

// ❌ Sync-over-async anti-pattern
public void OnSaveClicked()
{
    var dcr = _service.GetAsync(id).Result;  // ❌ BLOCKS UI THREAD!
}

// ✅ Use async event handlers
private async void OnSaveClickedAsync(object sender, EventArgs e)
{
    var dcr = await _service.GetAsync(id);  // ✅
}
```

---

## 🗂️ Architecture Rules

### Layer Boundaries

```csharp
// ✅ CORRECT: Dependency flow (top depends on below)
// UI → Application → Infrastructure → Domain

// ❌ WRONG: Domain depending on Application
namespace DCRManagement.Domain.Services
{
    public class MyService  // ❌ Services belong in Application layer!
    {
        public void DoSomething() { }
    }
}

// ✅ CORRECT: Services in Application layer
namespace DCRManagement.Application.Services
{
    public class DCRService
    {
        private readonly IDCRRepository _repo;  // ✅ Interface from Domain
        
        public async Task<Result<DCRDto>> CreateAsync(CreateDCRDto dto)
        {
            // ... business logic
        }
    }
}

// ❌ WRONG: Infrastructure class in Application
namespace DCRManagement.Application.Services
{
    public class SomeService
    {
        private EmailService _email;  // ❌ Infrastructure class exposed!
    }
}

// ✅ CORRECT: Use interfaces from Domain
namespace DCRManagement.Application.Services
{
    public class SomeService
    {
        private readonly IEmailService _email;  // ✅ Interface from Domain
    }
}
```

### DTO Layer Boundaries

```csharp
// ✅ CORRECT: Services accept/return DTOs, not entities
public class DCRService
{
    public async Task<Result<DCRDto>> CreateAsync(CreateDCRDto dto)  // ✅
    {
        var dcr = new DCR { ... };
        return dcr;  // Maps to DCRDto
    }
}

// ❌ WRONG: Exposing entities through service layer
public class BadService
{
    public async Task<DCR> CreateAsync(DCR dcr)  // ❌ Entity in public API
    {
        return dcr;
    }
}

// ✅ CORRECT: UI receives DTOs, not entities
var result = await _dcrService.CreateAsync(dto);
if (result.IsSuccess)
{
    var dcrDto = result.Value;  // ✅ DTO, safe to use in UI
}
```

### MVP Pattern (UI)

```csharp
// ✅ View defines interface
public interface IDCRDetailView : IView
{
    // Data binding
    string Title { get; set; }
    string Description { get; set; }
    
    // Events
    event EventHandler SaveRequested;
    event EventHandler<ApprovalAction> ActionRequested;
    
    // Display
    void SetMode(DetailMode mode);
    void ShowSuccess(string message);
    void ShowError(string message);
}

// ✅ Form implements view (ZERO LOGIC)
public partial class DCRDetailForm : Form, IDCRDetailView
{
    private DCRDetailPresenter _presenter;
    
    public string Title
    {
        get => _txtTitle.Text;
        set => _txtTitle.Text = value;
    }
    
    public event EventHandler SaveRequested;
    
    private void OnBtnSaveClick(object sender, EventArgs e)
    {
        SaveRequested?.Invoke(this, EventArgs.Empty);  // ✅ Just emit event
    }
    
    // ❌ NO LOGIC HERE!
}

// ✅ Presenter contains logic
public class DCRDetailPresenter
{
    private readonly IDCRDetailView _view;
    private readonly DCRService _service;
    
    public DCRDetailPresenter(IDCRDetailView view, DCRService service, ...)
    {
        _view = view;
        _view.SaveRequested += OnSaveAsync;
    }
    
    private async Task OnSaveAsync()
    {
        var dto = new CreateDCRDto(
            _view.Title,
            _view.Description,
            ...
        );
        
        var result = await _service.CreateAsync(dto);
        if (result.IsSuccess)
            _view.ShowSuccess("Saved!");
        else
            _view.ShowError(result.ErrorMessage!);
    }
}
```

---

## 🧪 Testing Standards

### Unit Test Naming

```csharp
[Fact]
public async Task MethodName_WithCondition_ExpectedResult()
{
    // Arrange
    var mockRepo = new Mock<IDCRRepository>();
    var service = new DCRService(mockRepo.Object, ...);
    
    // Act
    var result = await service.CreateAsync(new CreateDCRDto(...));
    
    // Assert
    Assert.True(result.IsSuccess);
}
```

### Test Structure

```csharp
// ✅ Arrange-Act-Assert pattern
[Fact]
public async Task ExecuteAction_WithInvalidTransition_ReturnsFail()
{
    // Arrange: set up test data
    var dcr = new DCR { Status = DCRStatus.Closed };
    var mockRepo = new Mock<IDCRRepository>();
    mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dcr);
    
    // Act: execute method
    var service = new WorkflowService(mockRepo.Object, ...);
    var result = await service.ExecuteActionAsync(1, ApprovalAction.Approve, 1);
    
    // Assert: verify results
    Assert.False(result.IsSuccess);
    Assert.Equal("INVALID_TRANSITION", result.ErrorCode);
    mockRepo.Verify(r => r.UpdateAsync(It.IsAny<DCR>()), Times.Never);
}
```

---

## 📝 Documentation Standards

### XML Comments for Public APIs

```csharp
/// <summary>
/// Creates a new DCR in Draft status. DCR number is auto-generated.
/// </summary>
/// <param name="dto">DTO containing DCR details</param>
/// <returns>Result containing created DCR DTO or error message</returns>
/// <exception cref="InvalidOperationException">Thrown if database fails (infrastructure error)</exception>
public async Task<Result<DCRDto>> CreateAsync(CreateDCRDto dto)
{
    // ...
}
```

---

## 🔗 Configuration Standards

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=...;..."
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "...",
    "SenderName": "DCR System",
    "Password": "..."
  }
}
```

---

## 🚫 Anti-Patterns to Avoid

| ❌ Anti-Pattern | ✅ Correct |
|---|---|
| `Task.Result` or `.Wait()` | Use `await` |
| `new Repository()` in service | Use dependency injection |
| `throw new Exception()` for business errors | Return `Result<T>.Failure(...)` |
| Logic in Forms/Views | Move to Presenters |
| Global static state (besides SessionContext) | Use DI or Singleton service |
| Sync-over-async | Always use async/await |
| Try-catch all exceptions silently | Log and handle by layer |
| Entity in public API | Use DTO |
| N+1 queries | Use `.Include()` for related data |

---

## 📚 References

- [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [Clean Code Book](https://www.oreilly.com/library/view/clean-code-a/9780136083238/)
- [Architecture Overview](architecture/system-overview.md)
- [Development Guide](DEVELOPMENT_GUIDE.md)
