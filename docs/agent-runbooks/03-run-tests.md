# Runbook 03 — Chạy Full Test Suite

> Agent chạy runbook này để verify toàn bộ test suite pass trước khi commit hoặc deploy.

## Thứ tự thực hiện

```
1. Build solution (không có lỗi)
2. Chạy unit tests (WorkflowService, DCRService, AuthService)
3. Chạy integration tests (Repository với InMemory DB)
4. Kiểm tra coverage (optional)
5. Báo cáo kết quả
```

---

## Step 1 — Build trước khi test

```bash
dotnet build DCRManagement.sln --configuration Debug --no-restore
# Phải: Build succeeded, 0 Error(s)
```

## Step 2 — Chạy toàn bộ tests

```bash
dotnet test tests/DCRManagement.Tests/ \
  --configuration Debug \
  --logger "console;verbosity=normal" \
  --no-build
```

## Step 3 — Chạy theo nhóm (nếu một nhóm fail)

```bash
# Chỉ workflow tests
dotnet test --filter "FullyQualifiedName~WorkflowServiceTests"

# Chỉ DCRService tests
dotnet test --filter "FullyQualifiedName~DCRServiceTests"

# Chỉ repository tests
dotnet test --filter "FullyQualifiedName~RepositoryTests"

# Chỉ auth tests
dotnet test --filter "FullyQualifiedName~AuthServiceTests"
```

## Step 4 — Code Coverage

```bash
dotnet test tests/DCRManagement.Tests/ \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults

# Generate HTML report
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator \
  -reports:"./TestResults/**/coverage.cobertura.xml" \
  -targetdir:"./TestResults/CoverageReport" \
  -reporttypes:Html

# Mở report
start ./TestResults/CoverageReport/index.html  # Windows
open ./TestResults/CoverageReport/index.html   # Mac
```

---

## Danh sách tests phải PASS

### WorkflowService (18 tests)

| Test | Mô tả |
|------|-------|
| `Submit_Draft_TransitionsToPendingReview` | Happy path submit |
| `Submit_ApprovedDCR_ReturnsInvalidTransition` | Invalid state |
| `Submit_ReadOnlyRole_ReturnsUnauthorized` | Role check |
| `StartReview_NotAssignedReviewer_ReturnsNotAssigned` | Assignment check |
| `StartReview_AssignedReviewer_Succeeds` | Happy path review |
| `SendToApproval_UnderReview_Succeeds` | Review → Approval |
| `Approve_AssignedApprover_Succeeds` | Happy path approve |
| `Approve_WrongApprover_ReturnsNotAssigned` | Wrong approver |
| `Reject_AnyNonTerminalState_Succeeds` | Reject from multiple states |
| `Cancel_Draft_Succeeds` | Cancel from draft |
| `Cancel_PendingReview_Succeeds` | Cancel from pending |
| `Close_Approved_SetsCompletionDate` | Auto-set date |
| `Close_Approved_TransitionsToClosed` | Terminal state |
| `DCRNotFound_ReturnsFailure` | Not found |
| `ActorNotFound_ReturnsFailure` | Actor not found |
| `AdminCanPerformAnyAction` | Admin bypass |
| `GetAvailableActions_DraftEngineer_ReturnsSubmitAndCancel` | UI actions |
| `GetAvailableActions_PendingReviewAdmin_ReturnsAllActions` | Admin actions |

### DCRService (10 tests)

| Test | Mô tả |
|------|-------|
| `Create_ValidDto_ReturnsDCRDto` | Happy path |
| `Create_EmptyTitle_ReturnsValidationError` | Validation |
| `Create_PastTargetDate_ReturnsValidationError` | Date validation |
| `Create_GeneratesDCRNumber` | Auto-numbering |
| `Update_Draft_Succeeds` | Edit draft |
| `Update_Approved_ReturnsNotEditable` | Edit restriction |
| `Update_NotOwner_ReturnsUnauthorized` | Ownership check |
| `Submit_ValidReviewerApprover_Succeeds` | Submit flow |
| `Submit_WrongRole_ReturnsInvalidReviewer` | Role validation |
| `GetById_NotFound_ReturnsFailure` | Not found |

### AuthService (5 tests)

| Test | Mô tả |
|------|-------|
| `Login_ValidCredentials_PopulatesSession` | Happy path |
| `Login_WrongPassword_ReturnsInvalid` | Bad password |
| `Login_InactiveAccount_ReturnsInactive` | Deactivated |
| `Login_EmptyUsername_ReturnsEmptyCredentials` | Validation |
| `Logout_ClearsSession` | Session clear |

### DCRRepository Integration (5 tests)

| Test | Mô tả |
|------|-------|
| `GenerateNumber_Empty_Returns0001` | First DCR |
| `GenerateNumber_ExistingEntries_Increments` | Sequence |
| `GetWithFullDetails_IncludesHistory` | Eager loading |
| `GetAssignedToUser_ReturnsCorrectDCRs` | Assignment filter |
| `SoftDelete_FilteredFromAllQueries` | Global filter |

---

## Expected: Test Summary

```
Test Run Successful.
Total tests: 38
     Passed: 38
     Failed: 0
  Skipped: 0
 Total time: ~2.5 Seconds
```

## Khi có test FAIL

1. Đọc kỹ error message và stack trace
2. Xác định layer bị fail (Service? Repository? Entity?)
3. Tham khảo `SKILL-debug.md` cho các lỗi thường gặp
4. Fix code → re-run test cụ thể đó trước:
   ```bash
   dotnet test --filter "FullyQualifiedName~<TênTest>"
   ```
5. Sau khi pass → chạy lại full suite

## CI Integration

Thêm vào GitHub Actions / Azure DevOps:

```yaml
# .github/workflows/ci.yml
- name: Run Tests
  run: |
    dotnet restore
    dotnet build --no-restore
    dotnet test tests/DCRManagement.Tests/ --no-build --logger "trx;LogFileName=test-results.trx"

- name: Publish Test Results
  uses: actions/upload-artifact@v3
  with:
    name: test-results
    path: '**/test-results.trx'
```
