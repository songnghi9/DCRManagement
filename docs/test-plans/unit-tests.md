# Unit Test Plan — DCRManagement

## Mục tiêu coverage

| Layer | Target Coverage |
|-------|----------------|
| Domain Entities | 80% |
| Application Services | 90% |
| Infrastructure Repositories | 70% (integration tests) |
| UI Presenters | 60% |

---

## 1. WorkflowService Tests

**File:** `tests/DCRManagement.Tests/WorkflowServiceTests.cs`

### Happy path transitions

```
[Fact] Submit_DraftByEngineer_TransitionsToPendingReview
[Fact] Submit_RejectedByEngineer_TransitionsToPendingReview  ← re-submission
[Fact] StartReview_PendingByAssignedReviewer_TransitionsToUnderReview
[Fact] SendToApproval_UnderReviewByAssignedReviewer_TransitionsToPendingApproval
[Fact] Approve_PendingByAssignedApprover_TransitionsToApproved
[Fact] Reject_PendingReviewByReviewer_TransitionsToRejected
[Fact] Reject_PendingApprovalByApprover_TransitionsToRejected
[Fact] Close_ApprovedByAdmin_TransitionsToClosed
[Fact] Cancel_DraftByEngineer_TransitionsToCancelled
[Fact] Cancel_PendingReviewByEngineer_TransitionsToCancelled
```

### Authorization failures

```
[Fact] Submit_ByReadOnlyRole_ReturnsUnauthorized
[Fact] Approve_ByReviewerRole_ReturnsUnauthorized
[Fact] StartReview_ByEngineerRole_ReturnsUnauthorized
[Fact] Reject_ByEngineerRole_ReturnsUnauthorized
```

### State machine violations

```
[Fact] Submit_AlreadyPendingReview_ReturnsInvalidTransition
[Fact] Submit_Approved_ReturnsInvalidTransition
[Fact] Approve_Draft_ReturnsInvalidTransition
[Fact] Close_PendingApproval_ReturnsInvalidTransition
```

### Assignment checks

```
[Fact] StartReview_NotAssignedReviewer_ReturnsNotAssigned
[Fact] Approve_NotAssignedApprover_ReturnsNotAssigned
[Fact] Admin_BypassesAssignmentCheck
```

### Side effects

```
[Fact] ExecuteAction_RecordsApprovalHistory
[Fact] ExecuteAction_UpdatesUpdatedAt
[Fact] Close_SetsActualCompletionDate
[Fact] ExecuteAction_TriggersEmailNotification
[Fact] ExecuteAction_EmailFails_DoesNotRollbackWorkflow
```

### Edge cases

```
[Fact] ExecuteAction_DCRNotFound_ReturnsFailure
[Fact] ExecuteAction_ActorNotFound_ReturnsFailure
[Fact] GetAvailableActions_DraftEngineer_ReturnsSubmitAndCancel
[Fact] GetAvailableActions_Closed_ReturnsEmpty
[Fact] GetAvailableActions_AdminAllStates_ReturnsAllValidActions
```

---

## 2. DCRService Tests

**File:** `tests/DCRManagement.Tests/DCRServiceTests.cs`

### CreateAsync

```
[Fact] Create_ValidDto_ReturnsDCRWithGeneratedNumber
[Fact] Create_EmptyTitle_ReturnsValidationError
[Fact] Create_TitleOver300Chars_ReturnsValidationError
[Fact] Create_EmptyDescription_ReturnsValidationError
[Fact] Create_PastTargetDate_ReturnsValidationError
[Fact] Create_NullOptionalFields_Succeeds
[Fact] Create_SetsCreatedByFromSession
[Fact] Create_SetsDraftStatus
```

### UpdateAsync

```
[Fact] Update_DraftByOwner_Succeeds
[Fact] Update_RejectedByOwner_Succeeds
[Fact] Update_ApprovedDCR_ReturnsNotEditable
[Fact] Update_UnderReviewDCR_ReturnsNotEditable
[Fact] Update_ByNonOwner_ReturnsUnauthorized
[Fact] Update_ByAdmin_SucceedsRegardlessOfOwner
[Fact] Update_NotFound_ReturnsFailure
```

### SubmitAsync

```
[Fact] Submit_ValidReviewerAndApprover_Succeeds
[Fact] Submit_AssignsReviewerAndApproverBeforeTransition
[Fact] Submit_InvalidReviewer_ReturnsError
[Fact] Submit_WrongRole_ReturnsInvalidReviewer
[Fact] Submit_DCRNotFound_ReturnsFailure
[Fact] Submit_ApprovedDCR_ReturnsInvalidState
```

### GetByIdAsync / GetAllForCurrentUser

```
[Fact] GetById_NotFound_ReturnsFailure
[Fact] GetById_ExistingDCR_ReturnsMappedDto
[Fact] GetAllForCurrentUser_Admin_ReturnsAll
[Fact] GetAllForCurrentUser_Engineer_ReturnsOwnAndAssigned
[Fact] GetAllForCurrentUser_Reviewer_ReturnsAssignedDCRs
```

---

## 3. AuthService Tests

**File:** `tests/DCRManagement.Tests/AuthServiceTests.cs`

```
[Fact] Login_CorrectCredentials_PopulatesSessionContext
[Fact] Login_WrongPassword_ReturnsInvalidCredentials
[Fact] Login_UnknownUsername_ReturnsInvalidCredentials
[Fact] Login_InactiveUser_ReturnsAccountInactive
[Fact] Login_EmptyUsername_ReturnsEmptyCredentials
[Fact] Login_EmptyPassword_ReturnsEmptyCredentials
[Fact] Logout_ClearsSessionContext
```

---

## 4. UserService Tests

**File:** `tests/DCRManagement.Tests/UserServiceTests.cs`

```
[Fact] CreateUser_UniqueUsername_Succeeds
[Fact] CreateUser_DuplicateUsername_ReturnsDuplicateError
[Fact] CreateUser_DuplicateEmail_ReturnsDuplicateError
[Fact] CreateUser_HashesPassword
[Fact] UpdateUser_ValidData_Succeeds
[Fact] UpdateUser_SelfDeactivation_ReturnsError
[Fact] ChangePassword_CorrectCurrent_Succeeds
[Fact] ChangePassword_WrongCurrent_ReturnsWrongPassword
[Fact] ChangePassword_WeakNew_ReturnsWeakPassword
[Fact] GetReviewers_ReturnsOnlyReviewerRole
[Fact] GetApprovers_ReturnsOnlyApproverRole
```

---

## 5. Repository Integration Tests (InMemory DB)

**File:** `tests/DCRManagement.Tests/Repositories/DCRRepositoryTests.cs`

```
[Fact] GenerateNumber_EmptyDB_ReturnsDCRCurrentYear0001
[Fact] GenerateNumber_ExistingEntries_Increments
[Fact] GetWithFullDetails_IncludesHistorySortedDescending
[Fact] GetWithFullDetails_IncludesAttachments
[Fact] GetAssignedToUser_ReviewerOrApprover_ReturnsBoth
[Fact] GetByStatus_FiltersByStatus
[Fact] GetByCreator_FiltersByCreatedById
[Fact] SoftDeleteFilter_ExcludesDeletedRecords
```

---

## 6. Result<T> Tests

**File:** `tests/DCRManagement.Tests/Common/ResultTests.cs`

```
[Fact] Success_IsSuccessTrue_HasValue
[Fact] Failure_IsSuccessFalse_HasErrorMessage
[Fact] Failure_WithErrorCode_HasBothFields
[Fact] ImplicitConversion_ValueToResult_IsSuccess
[Fact] NonGenericResult_Ok_IsSuccess
[Fact] NonGenericResult_Failure_HasMessage
```

---

## Test Helpers (tạo sẵn để tái sử dụng)

```csharp
// tests/DCRManagement.Tests/Helpers/TestFixtures.cs

public static class TestFixtures
{
    public static DCR CreateDraftDCR(int id = 1, int createdById = 1) => new DCR
    {
        Id = id,
        DCRNumber = $"DCR-{DateTime.Now.Year}-{id:D4}",
        Title = "Test DCR",
        Description = "Test Description",
        Status = DCRStatus.Draft,
        CreatedById = createdById,
        CreatedAt = DateTime.UtcNow
    };

    public static User CreateUser(int id, UserRole role, string? username = null) => new User
    {
        Id = id,
        Username = username ?? $"user{id}",
        FullName = $"User {id}",
        Email = $"user{id}@test.com",
        Role = role,
        IsActive = true,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
        CreatedById = 1
    };

    public static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}
```
