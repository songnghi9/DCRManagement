# Refactoring Opportunities — DCRManagement

> Tài liệu này ghi lại các cơ hội refactor để cải thiện chất lượng code, hiệu suất, và bảo trì dễ dàng.

---

## 🔴 High Priority

### 1. **Async All The Way — Remove Sync-over-Async Anti-pattern**

**Current Status:** ❌ Not detected yet, but watch for this pattern

```csharp
// ❌ ANTI-PATTERN: Never do this
public DCRDto GetDCR(int id)
{
    return _service.GetDCRAsync(id).Result;  // BLOCKS THREAD!
}

// ✅ CORRECT: Always await
public async Task<DCRDto> GetDCRAsync(int id)
{
    return await _service.GetDCRAsync(id);
}
```

**Action:** Monitor code reviews to catch and fix immediately

---

### 2. **Repository Pattern — Add Query Optimization Layer**

**Issue:** Some queries don't use `.Include()` causing N+1 queries

**Example:**
```csharp
// ❌ Current approach
var dcrs = await _dcrRepository.GetAllAsync();  // No includes!

foreach (var dcr in dcrs)
{
    var reviewer = dcr.AssignedReviewer;  // ← N+1 query per DCR
}
```

**Fix:** Add specialized query methods with explicit includes
```csharp
// ✅ Optimized
public async Task<IEnumerable<DCRWithDetailsDto>> GetAllWithDetailsAsync()
{
    return await _dbSet
        .Include(d => d.AssignedReviewer)
        .Include(d => d.AssignedApprover)
        .Include(d => d.ApprovalHistories)
        .Select(d => new DCRWithDetailsDto(...))
        .ToListAsync();
}
```

**Priority:** Medium (do after MVP works)  
**Estimate:** 4-6 hours

---

### 3. **Validation Layer — Extract to FluentValidation**

**Current:** Scattered validation logic in services

```csharp
// ❌ Current
public async Task<Result<DCRDto>> CreateAsync(CreateDCRDto dto)
{
    var validationResult = ValidateCreateDto(dto);  // ← Where is this?
    if (!validationResult.IsSuccess)
        return Result<DCRDto>.Failure(...);
```

**Better:** Use FluentValidation for reusable, testable validators

```csharp
// ✅ Recommended
public class CreateDCRDtoValidator : AbstractValidator<CreateDCRDto>
{
    public CreateDCRDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title max 200 chars");
            
        RuleFor(x => x.Description)
            .NotEmpty()
            .MinimumLength(10);
    }
}

// In service:
public async Task<Result<DCRDto>> CreateAsync(CreateDCRDto dto)
{
    var validator = new CreateDCRDtoValidator();
    var validation = await validator.ValidateAsync(dto);
    
    if (!validation.IsValid)
        return Result<DCRDto>.Failure(string.Join("; ", validation.Errors));
    
    // ... proceed
}
```

**Benefits:** 
- Centralized validation rules
- Reusable in API + UI layers
- Easy to test

**Priority:** Medium  
**Estimate:** 3-4 hours

---

## 🟡 Medium Priority

### 4. **Logging — Move from String Interpolation to Structured Logging**

**Current:**
```csharp
// ❌ Harder to parse and filter
_logger.LogInformation($"User {username} created DCR {dcrNumber}");
```

**Better:**
```csharp
// ✅ Structured: parseable by log aggregation tools
_logger.LogInformation("User {Username} created DCR {DCRNumber}", username, dcrNumber);
```

**Action:** 
1. Audit all log statements
2. Replace string interpolation with positional parameters
3. Consider adding correlation IDs for request tracing

**Priority:** Medium  
**Estimate:** 2-3 hours

---

### 5. **Exception Handling — Implement Global Handler**

**Issue:** UI catches exceptions individually; no consistent error display

```csharp
// ❌ Repeated in multiple forms
catch (Exception ex)
{
    _logger.LogError(ex, "...");
    MessageBox.Show("An error occurred");
}
```

**Solution:** Add global exception handler

```csharp
// ✅ In Program.cs
Application.ThreadException += (s, e) =>
{
    _logger.LogError(e.Exception, "Unhandled UI exception");
    MessageBox.Show("An unexpected error occurred. Please contact support.");
};

AppDomain.CurrentDomain.UnhandledException += (s, e) =>
{
    _logger.LogError((Exception)e.ExceptionObject, "Unhandled domain exception");
};
```

**Priority:** Medium  
**Estimate:** 2 hours

---

### 6. **Database Transactions — Add for Workflow Operations**

**Current:** Multi-step operations (update DCR + create history + send email) lack atomicity

**Issue:**
```csharp
// ❌ If email fails after SaveChanges, history is orphaned
await _dcrRepository.UpdateAsync(dcr);
await _historyRepository.AddAsync(history);  // Committed already
_ = EmailService.SendAsync(...);  // Fails silently
```

**Fix:** Use transactions
```csharp
// ✅ Atomic operation
using var transaction = _context.Database.BeginTransaction();
try
{
    await _dcrRepository.UpdateAsync(dcr);
    await _historyRepository.AddAsync(history);
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

**Priority:** Medium-High (prevents data corruption)  
**Estimate:** 3-4 hours

---

## 🟢 Low Priority (Nice-to-Have)

### 7. **Caching — Add Redis/In-Memory Caching for Reference Data**

**Example:**
```csharp
// ✅ Cache user dropdown list (rarely changes)
public async Task<IEnumerable<UserSummaryDto>> GetReviewersAsync()
{
    return await _cache.GetOrCreateAsync("reviewers", async entry =>
    {
        entry.SetSlidingExpiration(TimeSpan.FromHours(1));
        return await _userRepository.GetByRoleAsync(UserRole.Reviewer);
    });
}
```

**Priority:** Low (doesn't impact MVP)  
**Estimate:** 4-6 hours

---

### 8. **API Layer — Add REST API Alongside WinForms**

**Rationale:** Future mobile app or browser client could reuse business logic

```csharp
// ✅ Add DCRController in new project
[ApiController]
[Route("api/[controller]")]
public class DCRsController
{
    [HttpPost("create")]
    public async Task<ActionResult<DCRDto>> CreateDCR(CreateDCRDto dto)
    {
        var result = await _dcrService.CreateAsync(dto);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.ErrorMessage);
    }
}
```

**Priority:** Low (post-MVP)  
**Estimate:** 8-10 hours

---

### 9. **Audit Logging — Track Data Changes**

**Example:**
```csharp
// ✅ Automatically log all changes
public override async Task SaveChangesAsync(...)
{
    var entries = ChangeTracker.Entries()
        .Where(e => e.State == EntityState.Modified);
    
    foreach (var entry in entries)
    {
        // Log what changed
        var auditLog = new AuditLog
        {
            TableName = entry.Entity.GetType().Name,
            RecordId = (int)entry.Property("Id").CurrentValue,
            ChangedBy = SessionContext.Instance.UserId,
            ChangedAt = DateTime.UtcNow,
            Changes = GetChanges(entry)  // Track field diffs
        };
        
        await _auditRepository.AddAsync(auditLog);
    }
    
    return await base.SaveChangesAsync(cancellationToken);
}
```

**Priority:** Low (nice-to-have, not critical for MVP)  
**Estimate:** 6-8 hours

---

## 📋 Refactoring Checklist

### Before Each Refactoring
- [ ] Write unit tests for the code you're refactoring
- [ ] Ensure tests pass before refactoring
- [ ] Refactor one concern at a time (don't mix refactors)
- [ ] Ensure tests still pass after refactoring
- [ ] Create git commit with clear message

### Code Review Checklist
- [ ] No `Task.Result` or `.Wait()` (unless explicitly needed)
- [ ] No `catch (Exception ex) { }` without logging
- [ ] All `async Task` methods have proper error handling
- [ ] DTOs don't contain business logic
- [ ] Services don't directly instantiate dependencies
- [ ] Tests don't use real external services (use mocks)

---

## 🎯 Priority Road Map

### Phase 1 (Current Sprint)
- ✅ Fix configuration key mismatch
- ✅ Fix UI startup
- Add unit test skeleton

### Phase 2 (Next Sprint)
- Extract validation to FluentValidation
- Add global exception handler
- Add N+1 query optimization

### Phase 3 (Later)
- Add API layer
- Implement caching
- Add audit logging
- Structured logging improvements

---

## Related Files
- [Architecture Overview](architecture/system-overview.md)
- [Development Guide](DEVELOPMENT_GUIDE.md)
- [Changes Log](CHANGES.md)
