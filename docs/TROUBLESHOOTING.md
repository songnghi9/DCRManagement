# Troubleshooting Guide — DCRManagement

> Danh sách các vấn đề thường gặp và giải pháp.

---

## 🔴 Build Errors

### Error: "The type or namespace name 'X' could not be found"

**Cause:** Missing using statement or project reference

**Solution:**
```csharp
// Add missing using
using DCRManagement.Application.Services;

// Or check project references:
// - UI → Application, Infrastructure
// - Application → Domain
// - Infrastructure → Domain, Application
```

**Check:**
```bash
dotnet restore DCRManagement.sln
```

---

### Error: "CS0246: The type or namespace name 'Result' could not be found"

**Cause:** Forgot to include the namespace

**Solution:**
```csharp
// Add at top of file
using DCRManagement.Application.Common;

// Then use
public async Task<Result<DCRDto>> CreateAsync(CreateDCRDto dto)
{
    if (invalid)
        return Result<DCRDto>.Failure("Error message", "ERROR_CODE");
    
    return dto;  // Implicit conversion to Result<DCRDto>
}
```

---

### Error: "The DbContext cannot be used while the model is being created"

**Cause:** Accessing database during AppDbContext initialization

**Solution:** Don't query database in `OnConfiguring` or `OnModelCreating`:
```csharp
// ❌ WRONG
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    var config = _context.Configurations.FirstOrDefault();  // ❌ Database access!
}

// ✅ CORRECT
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}
```

---

## 🟡 Runtime Errors

### Error: "ConnectionString 'DefaultConnection' is not defined"

**Cause:** appsettings.json missing or incorrect location

**Solution:**

1. **Verify file exists:**
   ```bash
   ls src/DCRManagement.UI/appsettings.json
   ```

2. **Update .csproj to copy file:**
   ```xml
   <!-- UI.csproj -->
   <ItemGroup>
       <None Update="appsettings.json">
           <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
       </None>
   </ItemGroup>
   ```

3. **Rebuild:**
   ```bash
   dotnet clean DCRManagement.sln
   dotnet build DCRManagement.sln
   ```

---

### Error: "Invalid object name 'dbo.DCRs'" (Database not created)

**Cause:** Migrations not applied

**Solution:**
```bash
# Apply migrations
dotnet ef database update --project src/DCRManagement.Infrastructure --startup-project src/DCRManagement.UI

# Check migration status
dotnet ef migrations list --project src/DCRManagement.Infrastructure

# Verify database created
sqlcmd -S (localdb)\mssqllocaldb -Q "SELECT NAME FROM sys.databases WHERE NAME='DCRManagement'"
```

---

### Error: "There is already an open DataReader associated with this connection"

**Cause:** Executing query while enumerating previous result

**Solution:**
```csharp
// ❌ WRONG
foreach (var dcr in _context.DCRs)
{
    var users = _context.Users.ToList();  // ❌ Another query while loop active
}

// ✅ CORRECT
var dcrs = _context.DCRs.ToList();  // Execute and materialize first
foreach (var dcr in dcrs)
{
    var users = _context.Users.ToList();  // Now safe
}

// OR use .Include() in original query
var dcrWithUsers = _context.DCRs
    .Include(d => d.CreatedBy)  // ✅ Load related data in one query
    .ToList();
```

---

### Error: "NullReferenceException: Object reference not set to an instance of an object"

**Common Causes & Solutions:**

#### 1. SessionContext.Instance not initialized (user not logged in)

```csharp
// ❌ WRONG: Accessing without checking
var userId = SessionContext.Instance.UserId;  // May be 0 if not logged in

// ✅ CORRECT: Check first
if (!SessionContext.Instance.IsAuthenticated)
{
    MessageBox.Show("Please login first");
    return;
}

var userId = SessionContext.Instance.UserId;
```

#### 2. Repository returns null

```csharp
// ❌ WRONG: Assuming entity exists
var dcr = await _dcrRepository.GetByIdAsync(id);
var title = dcr.Title;  // ❌ NullReferenceException if not found

// ✅ CORRECT: Check for null
var dcr = await _dcrRepository.GetByIdAsync(id);
if (dcr is null)
{
    MessageBox.Show("DCR not found");
    return;
}

var title = dcr.Title;
```

#### 3. Navigation property not loaded

```csharp
// ❌ WRONG: Assuming related data is loaded
var dcr = await _dcrRepository.GetByIdAsync(id);
var reviewerName = dcr.AssignedReviewer.FullName;  // ❌ NullReferenceException

// ✅ CORRECT: Use GetWithFullDetailsAsync or explicitly include
var dcr = await _dcrRepository.GetWithFullDetailsAsync(id);
var reviewerName = dcr.AssignedReviewer?.FullName ?? "Unassigned";
```

---

### Error: "Object has been deleted"

**Cause:** Accessing entity after it was soft-deleted

**Solution:**
```csharp
// Soft-delete sets IsDeleted=true
// Query filters automatically exclude deleted records

// ✅ Deleted records are invisible
var dcr = await _dcrRepository.GetByIdAsync(id);  // Returns null if IsDeleted=true

// If you need to un-delete:
dcr.IsDeleted = false;
await _dcrRepository.UpdateAsync(dcr);

// To see deleted records (admin only):
var allIncludingDeleted = _context.DCRs.IgnoreQueryFilters().Where(d => d.IsDeleted).ToList();
```

---

## 🟢 Configuration & Setup Issues

### Issue: "Email not sending"

**Checklist:**

1. **appsettings.json configured?**
   ```json
   {
     "Email": {
       "SmtpHost": "smtp.gmail.com",
       "SmtpPort": 587,
       "SenderEmail": "your-email@gmail.com",
       "SenderName": "DCR System",
       "Password": "your-app-password"  // Use app password for Gmail
     }
   }
   ```

2. **Gmail 2FA enabled?**
   - Use App Password (not account password)
   - Go to myaccount.google.com → Security → App passwords

3. **Network access?**
   ```bash
   ping smtp.gmail.com  # Should respond
   ```

4. **Enable detailed logging:**
   ```csharp
   // In Program.cs
   .AddFilter("MailKit.Net.Smtp", LogLevel.Debug)
   ```

5. **Test email sending:**
   ```csharp
   // In a test form
   var emailService = serviceProvider.GetRequiredService<IEmailService>();
   await emailService.SendAsync("test@example.com", "Test", "<p>Test email</p>");
   ```

---

### Issue: "Database connection fails"

**Checklist:**

1. **SQL Server running?**
   ```bash
   # For LocalDB
   SqlLocalDB.exe info
   
   # Start if needed
   SqlLocalDB.exe start mssqllocaldb
   ```

2. **Connection string correct?**
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=DCRManagement;Trusted_Connection=true;"
     }
   }
   ```

3. **Test connection:**
   ```bash
   sqlcmd -S (localdb)\mssqllocaldb -Q "SELECT @@VERSION"
   ```

4. **Check firewall:**
   - If using full SQL Server (not LocalDB)
   - Ensure port 1433 not blocked

---

### Issue: "Migrations folder not found"

**Solution:**
```bash
# Add migrations folder and initial migration
dotnet ef migrations add InitialCreate --project src/DCRManagement.Infrastructure --startup-project src/DCRManagement.UI

# Then apply
dotnet ef database update --project src/DCRManagement.Infrastructure --startup-project src/DCRManagement.UI
```

---

## 🎨 UI Issues

### Issue: "Forms not appearing or blank"

**Checklist:**

1. **InitializeComponent() called?**
   ```csharp
   public partial class DCRDetailForm : Form
   {
       public DCRDetailForm()
       {
           InitializeComponent();  // ✅ MUST call this first
           // Then other setup
       }
   }
   ```

2. **Form.Designer.cs file exists?**
   - Each form should have two files: `FormName.cs` and `FormName.Designer.cs`
   - Never edit the `.Designer.cs` file manually

3. **Presenter initialized?**
   ```csharp
   public partial class DCRDetailForm : Form, IDCRDetailView
   {
       private DCRDetailPresenter _presenter;
       
       public DCRDetailForm(DCRService service, ...)
       {
           InitializeComponent();
           _presenter = new DCRDetailPresenter(this, service, ...);  // ✅
       }
   }
   ```

---

### Issue: "EventHandler not firing"

**Checklist:**

1. **Event subscribed in constructor?**
   ```csharp
   public DCRDetailPresenter(IDCRDetailView view, ...)
   {
       _view = view;
       _view.SaveRequested += OnSaveAsync;  // ✅ Subscribe
   }
   ```

2. **Event raised from view?**
   ```csharp
   public class DCRDetailForm : Form, IDCRDetailView
   {
       private void OnBtnSaveClick(object sender, EventArgs e)
       {
           SaveRequested?.Invoke(this, EventArgs.Empty);  // ✅ Raise event
       }
   }
   ```

3. **Handler has correct signature?**
   ```csharp
   // ✅ Correct signature
   private async Task OnSaveAsync()
   {
       // ...
   }
   
   // ❌ Wrong: not async or wrong params
   private void OnSave(int id)  // WRONG!
   {
   }
   ```

---

## 🧪 Testing Issues

### Error: "Tests fail with 'No database provider configured'"

**Solution:**
```csharp
// Use in-memory SQLite for tests
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlite("Data Source=:memory:")
    .Options;

using var context = new AppDbContext(options);
context.Database.EnsureCreated();

// Now test with real DbContext
var service = new DCRService(new DCRRepository(context), ...);
```

---

### Error: "Mock repository not working"

**Common Mistake:**
```csharp
// ❌ WRONG: Forgot to setup mock
var mockRepo = new Mock<IDCRRepository>();
var result = await mockRepo.Object.GetByIdAsync(1);  // Returns null!

// ✅ CORRECT: Setup mock to return data
mockRepo
    .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
    .ReturnsAsync(new DCR { Id = 1, Title = "Test" });

var result = await mockRepo.Object.GetByIdAsync(1);  // Returns DCR
```

---

## 🐛 Debugging Tips

### 1. Enable Debug Output

```csharp
// In Program.cs
services.AddLogging(config =>
    config
        .AddConsole()
        .AddDebug()  // ← Sends to Visual Studio Debug window
        .AddFilter("Microsoft", LogLevel.Warning)
        .AddFilter("Default", LogLevel.Debug)
);
```

### 2. Add Breakpoints & Inspect Variables

```csharp
// Press F5 to start debugging
// Click left margin to add breakpoint
var dcr = await _dcrRepository.GetByIdAsync(id);  // ← Add breakpoint here
// Hover over variables to inspect values
```

### 3. Check Entity State

```csharp
// In EF Core debugging
var entry = _context.Entry(dcr);
Console.WriteLine($"State: {entry.State}");  // Added, Modified, Unchanged, Deleted

// Check which properties changed
foreach (var prop in entry.Properties)
{
    if (prop.IsModified)
        Console.WriteLine($"{prop.Metadata.Name}: {prop.OriginalValue} → {prop.CurrentValue}");
}
```

### 4. SQL Query Debugging

```csharp
// Enable SQL logging
services.AddLogging(config =>
    config.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Debug)
);

// Now all queries print to console
```

---

## 📞 Getting Help

### Resources

- [Architecture Overview](architecture/system-overview.md)
- [Code Standards](CODE_STANDARDS.md)
- [Development Guide](DEVELOPMENT_GUIDE.md)
- [Changes Log](CHANGES.md)

### When Reporting Issues

Provide:
1. **Error message** (exact text)
2. **Stack trace** (if available)
3. **Steps to reproduce**
4. **Expected vs actual behavior**
5. **Code snippet** if relevant

---

## 🔗 Common Links

- [SQL Server LocalDB Docs](https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb)
- [Entity Framework Core Docs](https://learn.microsoft.com/en-us/ef/core/)
- [Moq Unit Testing Docs](https://github.com/moq/moq4/wiki/Quickstart)
- [Gmail App Passwords](https://support.google.com/accounts/answer/185833)
