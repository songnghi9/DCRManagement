# Maintenance & Operations Guide

> Operational guidelines for maintaining DCRManagement in production and development environments.

---

## 🔄 Regular Tasks

### Daily Checks (if applicable)

- [ ] Application logs for errors
- [ ] Database backups completed (if production)
- [ ] Email notifications sending properly

### Weekly Checks

- [ ] Code review queue
- [ ] Performance metrics
- [ ] Security updates available?
- [ ] NuGet package updates

### Monthly Checks

- [ ] NuGet package updates (major/minor)
- [ ] Database maintenance (integrity check, rebuilds)
- [ ] Documentation up-to-date?
- [ ] Refactoring opportunities addressed?

---

## 📊 Database Maintenance

### Backup Database

```bash
# Full backup (SQL Server Management Studio or T-SQL)
BACKUP DATABASE DCRManagement
TO DISK = 'C:\backups\DCRManagement.bak'
```

### Check Database Integrity

```bash
# In SQL Server Management Studio
DBCC CHECKDB (DCRManagement)
```

### View Database Size

```sql
SELECT
    DB_NAME(database_id) AS DatabaseName,
    CAST(SUM(size) * 8. / 1024 / 1024 AS DECIMAL(8,2)) AS SizeGB
FROM sys.master_files
GROUP BY database_id;
```

### Rebuild Indexes (if fragmented)

```sql
ALTER INDEX ALL ON DCRManagement.dbo.DCRs REBUILD;
ALTER INDEX ALL ON DCRManagement.dbo.Users REBUILD;
ALTER INDEX ALL ON DCRManagement.dbo.ApprovalHistories REBUILD;
```

### Reset Database (Development Only)

```bash
# ⚠️ WARNING: This deletes all data!
dotnet ef database drop --force --project src/DCRManagement.Infrastructure --startup-project src/DCRManagement.UI
dotnet ef database update --project src/DCRManagement.Infrastructure --startup-project src/DCRManagement.UI
```

---

## 🔐 Security Maintenance

### Update Dependencies

```bash
# Check for outdated packages
dotnet outdated DCRManagement.sln

# Update packages
dotnet package update --project src/DCRManagement.Infrastructure
dotnet package update --project src/DCRManagement.Application
```

### Update .NET Runtime

```bash
# Check current version
dotnet --version

# Update (if new LTS available)
# Download from https://dotnet.microsoft.com/en-us/download/dotnet
```

### Email Service Credentials (Gmail)

- [ ] App passwords don't expire
- [ ] Enable 2FA on email account
- [ ] Periodically rotate app password (quarterly)
- [ ] Don't commit passwords to repo (use appsettings)

### Database Connection String

- [ ] Use Windows authentication (Trusted_Connection) in Windows domain
- [ ] Use strong passwords if SQL authentication required
- [ ] Limit connection string in appsettings (don't expose passwords)
- [ ] Rotate SQL passwords periodically

---

## 📈 Performance Monitoring

### SQL Server Queries Performance

```sql
-- Find slow queries
SELECT TOP 10
    qs.execution_count,
    qs.total_elapsed_time / 1000000 AS total_seconds,
    qs.total_elapsed_time / qs.execution_count / 1000 AS avg_ms,
    SUBSTRING(st.text, 1, 100) AS query_text
FROM sys.dm_exec_query_stats qs
CROSS APPLY sys.dm_exec_sql_text(qs.sql_handle) st
ORDER BY qs.total_elapsed_time DESC;
```

### Enable Query Plans

```bash
# In appsettings.json, enable query logging
"Logging": {
    "LogLevel": {
        "Microsoft.EntityFrameworkCore.Database.Command": "Debug"
    }
}
```

### Monitor Disk Space

```bash
# Windows PowerShell
Get-Volume | Where-Object {$_.DriveType -eq 'Fixed'} | Select-Object DriveLetter, SizeRemaining, Size
```

---

## 🐛 Error Investigation

### Check Application Logs

```bash
# Logs are in console output or Event Viewer (Windows)
# Look for ERROR, CRITICAL level entries
```

### View Database Errors

```sql
-- SQL Server error log
EXEC sys.xp_readerrorlog 0;
```

### Debug Email Issues

```csharp
// Enable debug logging in appsettings.json
"Logging": {
    "LogLevel": {
        "MailKit.Net.Smtp": "Debug"
    }
}
```

---

## 🔄 Release Process

### Pre-Release Checklist

- [ ] All tests passing
- [ ] No compiler warnings
- [ ] Code reviewed
- [ ] Documentation updated
- [ ] Database migration tested
- [ ] Performance verified
- [ ] Security audit done

### Release Steps

```bash
# 1. Create release branch
git checkout -b release/v1.2.0

# 2. Update version numbers
# - Update DCRManagement.sln AssemblyVersion
# - Update package.json (if applicable)

# 3. Update CHANGELOG
# - Document new features
# - Document bug fixes
# - Document breaking changes

# 4. Create release tag
git tag -a v1.2.0 -m "Release version 1.2.0"

# 5. Merge to main
git checkout main
git merge release/v1.2.0

# 6. Push to origin
git push origin main
git push origin v1.2.0

# 7. Create release notes on GitHub
```

### Rollback Plan

```bash
# If release has critical issues
git revert <commit-hash>
git push origin main

# Or revert to previous tag
git checkout v1.1.0
git checkout -b hotfix/critical-issue
```

---

## 📝 Monitoring & Alerting

### Recommended Monitoring (Future)

- [ ] Application Insights (Azure)
- [ ] Sentry (Error tracking)
- [ ] New Relic (Performance)
- [ ] ELK Stack (Logging)

### Simple Monitoring (Current)

```csharp
// Add to Program.cs
services.AddLogging(config =>
    config
        .AddConsole()
        .AddFile("logs/app-{Date}.log")  // Requires Serilog.Sinks.File
);
```

---

## 🔧 Troubleshooting Common Issues

### Email Not Sending

1. Check SMTP credentials in appsettings.json
2. Verify Gmail app password (not account password)
3. Check firewall port 587 is open
4. Enable debug logging:
   ```json
   "Logging": {
       "LogLevel": {
           "MailKit": "Debug"
       }
   }
   ```

### High Database Disk Usage

1. Check table sizes:
   ```sql
   EXEC sp_spaceused;
   ```

2. Archive old ApprovalHistories:
   ```sql
   DELETE FROM ApprovalHistories WHERE ActionDate < DATEADD(YEAR, -1, GETDATE());
   DBCC SHRINKFILE (DCRManagement_log, 100);  -- Shrink log file
   ```

3. Rebuild indexes:
   ```sql
   ALTER INDEX ALL ON ApprovalHistories REBUILD;
   ```

### Slow Queries

1. Check execution plans
2. Add missing indexes (see SQL logs)
3. Review N+1 queries in code
4. Profile with SQL Server Profiler

---

## 📚 Maintenance Documentation

Keep updated:

- [CHANGES.md](docs/CHANGES.md) — Log all changes
- [REFACTORING_OPPORTUNITIES.md](docs/REFACTORING_OPPORTUNITIES.md) — Update with new findings
- [TROUBLESHOOTING.md](docs/TROUBLESHOOTING.md) — Add new issues & solutions
- [CODE_STANDARDS.md](docs/CODE_STANDARDS.md) — Update with lessons learned

---

## 🔗 Related

- [TROUBLESHOOTING.md](docs/TROUBLESHOOTING.md) — Error solutions
- [CODE_STANDARDS.md](docs/CODE_STANDARDS.md) — Coding standards
- [DEVELOPMENT_GUIDE.md](docs/DEVELOPMENT_GUIDE.md) — How to develop
- [SKILL-db.md](docs/skills/SKILL-db.md) — Database operations
