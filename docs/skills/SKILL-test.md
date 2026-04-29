---
name: dcr-test
description: Viết và chạy unit tests, integration tests cho DCRManagement. Dùng khi cần thêm test cases, chạy test suite, kiểm tra coverage, hoặc debug failing tests. Bao gồm mock setup cho repositories và services.
---

# Skill: Testing DCRManagement

## Stack test

- **Framework:** xUnit 2.9.2
- **Test project:** `tests/DCRManagement.Tests/`
- **Cần thêm packages:**
  - `Moq` — mock interfaces (IDCRRepository, IUserRepository, v.v.)
  - `Microsoft.EntityFrameworkCore.InMemory` — in-memory DB cho integration tests
  - `FluentAssertions` — assert dễ đọc hơn (optional)

## Cài packages cho test project

```xml
<!-- tests/DCRManagement.Tests/DCRManagement.Tests.csproj -->
<ItemGroup>
  <PackageReference Include="Moq" Version="4.20.72" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.0" />
  <PackageReference Include="FluentAssertions" Version="6.12.0" />
  <ProjectReference Include="..\..\src\DCRManagement.Application\DCRManagement.Application.csproj" />
  <ProjectReference Include="..\..\src\DCRManagement.Infrastructure\DCRManagement.Infrastructure.csproj" />
  <ProjectReference Include="..\..\src\DCRManagement.Domain\DCRManagement.Domain.csproj" />
</ItemGroup>
```

## Chạy tests

```bash
# Chạy tất cả
dotnet test tests/DCRManagement.Tests/

# Verbose output
dotnet test tests/DCRManagement.Tests/ --logger "console;verbosity=detailed"

# Chạy một test cụ thể
dotnet test --filter "FullyQualifiedName~WorkflowServiceTests"

# Chạy với coverage
dotnet test --collect:"XPlat Code Coverage"

# Kết quả HTML
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```

## Template: Unit Test cơ bản

```csharp
using DCRManagement.Application.Services;
using DCRManagement.Domain.Entities;
using DCRManagement.Domain.Enums;
using DCRManagement.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace DCRManagement.Tests;

public class WorkflowServiceTests
{
    private readonly Mock<IDCRRepository> _dcrRepoMock = new();
    private readonly Mock<IApprovalHistoryRepository> _historyRepoMock = new();
    private readonly Mock<IEmailService> _emailMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<ILogger<WorkflowService>> _loggerMock = new();

    private WorkflowService CreateSut() => new WorkflowService(
        _dcrRepoMock.Object,
        _historyRepoMock.Object,
        _emailMock.Object,
        _userRepoMock.Object,
        _loggerMock.Object);

    [Fact]
    public async Task ExecuteAction_Submit_TransitionsDraftToPendingReview()
    {
        // Arrange
        var dcr = new DCR { Id = 1, DCRNumber = "DCR-2024-0001", Status = DCRStatus.Draft };
        var actor = new User { Id = 10, Role = UserRole.Engineer, Username = "eng01", FullName = "Engineer One" };

        _dcrRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dcr);
        _userRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(actor);
        _historyRepoMock.Setup(r => r.AddAsync(It.IsAny<ApprovalHistory>()))
            .ReturnsAsync(new ApprovalHistory());

        var sut = CreateSut();

        // Act
        var result = await sut.ExecuteActionAsync(1, ApprovalAction.Submit, 10);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(DCRStatus.PendingReview, dcr.Status);
        _dcrRepoMock.Verify(r => r.UpdateAsync(dcr), Times.Once);
        _historyRepoMock.Verify(r => r.AddAsync(It.Is<ApprovalHistory>(h =>
            h.Action == ApprovalAction.Submit &&
            h.FromStatus == DCRStatus.Draft &&
            h.ToStatus == DCRStatus.PendingReview)), Times.Once);
    }

    [Fact]
    public async Task ExecuteAction_InvalidTransition_ReturnsFailure()
    {
        // Arrange — DCR đang ở Approved, không thể Submit
        var dcr = new DCR { Id = 1, Status = DCRStatus.Approved };
        var actor = new User { Id = 10, Role = UserRole.Engineer };

        _dcrRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dcr);
        _userRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(actor);

        var sut = CreateSut();

        // Act
        var result = await sut.ExecuteActionAsync(1, ApprovalAction.Submit, 10);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("INVALID_TRANSITION", result.ErrorCode);
        _dcrRepoMock.Verify(r => r.UpdateAsync(It.IsAny<DCR>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAction_WrongRole_ReturnsUnauthorized()
    {
        // Arrange — ReadOnly user cố Approve
        var dcr = new DCR { Id = 1, Status = DCRStatus.PendingApproval, AssignedApproverId = 99 };
        var actor = new User { Id = 10, Role = UserRole.ReadOnly };

        _dcrRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dcr);
        _userRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(actor);

        var sut = CreateSut();

        // Act
        var result = await sut.ExecuteActionAsync(1, ApprovalAction.Approve, 10);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("UNAUTHORIZED", result.ErrorCode);
    }

    [Fact]
    public async Task ExecuteAction_ReviewerNotAssigned_ReturnsNotAssigned()
    {
        // Arrange — Reviewer không phải người được assign
        var dcr = new DCR { Id = 1, Status = DCRStatus.PendingReview, AssignedReviewerId = 99 };
        var actor = new User { Id = 10, Role = UserRole.Reviewer };

        _dcrRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dcr);
        _userRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(actor);

        var sut = CreateSut();

        // Act
        var result = await sut.ExecuteActionAsync(1, ApprovalAction.StartReview, 10);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("NOT_ASSIGNED", result.ErrorCode);
    }

    [Fact]
    public async Task ExecuteAction_DCRNotFound_ReturnsFailure()
    {
        _dcrRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((DCR?)null);

        var sut = CreateSut();
        var result = await sut.ExecuteActionAsync(999, ApprovalAction.Submit, 1);

        Assert.False(result.IsSuccess);
        Assert.Equal("DCR_NOT_FOUND", result.ErrorCode);
    }

    [Fact]
    public async Task ExecuteAction_CloseApproved_SetsActualCompletionDate()
    {
        var dcr = new DCR { Id = 1, Status = DCRStatus.Approved };
        var actor = new User { Id = 10, Role = UserRole.Admin };

        _dcrRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dcr);
        _userRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(actor);
        _historyRepoMock.Setup(r => r.AddAsync(It.IsAny<ApprovalHistory>()))
            .ReturnsAsync(new ApprovalHistory());

        var sut = CreateSut();
        await sut.ExecuteActionAsync(1, ApprovalAction.Close, 10);

        Assert.NotNull(dcr.ActualCompletionDate);
        Assert.Equal(DCRStatus.Closed, dcr.Status);
    }
}
```

## Template: DCRService Tests

```csharp
public class DCRServiceTests
{
    private readonly Mock<IDCRRepository> _dcrRepoMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IApprovalHistoryRepository> _historyRepoMock = new();
    private readonly Mock<WorkflowService> _workflowMock;  // needs constructor args
    private readonly Mock<ILogger<DCRService>> _loggerMock = new();

    [Fact]
    public async Task CreateAsync_ValidDto_ReturnsDCRDto()
    {
        // Setup SessionContext
        SessionContext.Instance.Login(1, "eng", "Engineer", "eng@co.com", UserRole.Engineer);

        _dcrRepoMock.Setup(r => r.GenerateNextDCRNumberAsync())
            .ReturnsAsync("DCR-2024-0001");
        _dcrRepoMock.Setup(r => r.AddAsync(It.IsAny<DCR>()))
            .ReturnsAsync((DCR d) => d);
        _userRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new User { Id = 1, FullName = "Engineer" });

        // ... build service and assert
    }

    [Fact]
    public async Task CreateAsync_EmptyTitle_ReturnsValidationFailure()
    {
        var dto = new CreateDCRDto("", "Description", null, null, null, null, null);
        // ... assert result.IsSuccess == false, ErrorCode == "VALIDATION_ERROR"
    }
}
```

## Template: In-Memory Integration Test

```csharp
public class DCRRepositoryIntegrationTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly DCRRepository _repo;

    public DCRRepositoryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _repo = new DCRRepository(_context);
    }

    [Fact]
    public async Task GenerateNextDCRNumber_FirstEntry_ReturnsDCR2024_0001()
    {
        var number = await _repo.GenerateNextDCRNumberAsync();
        Assert.Equal($"DCR-{DateTime.Now.Year}-0001", number);
    }

    [Fact]
    public async Task GenerateNextDCRNumber_ExistingEntries_IncrementsSequence()
    {
        _context.DCRs.Add(new DCR { DCRNumber = $"DCR-{DateTime.Now.Year}-0003",
            Title = "T", Description = "D", CreatedById = 1 });
        await _context.SaveChangesAsync();

        var number = await _repo.GenerateNextDCRNumberAsync();
        Assert.Equal($"DCR-{DateTime.Now.Year}-0004", number);
    }

    public void Dispose() => _context.Dispose();
}
```

## Danh sách test cases cần viết (xem test-plans/unit-tests.md)
