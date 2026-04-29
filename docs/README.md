# DCRManagement — Agent Knowledge Base

Thư mục này chứa toàn bộ tài liệu để **agent AI tự động thực thi, build, test, refactor** hệ thống DCRManagement một cách chuyên nghiệp.

```
docs/
├── README.md                          ← Bạn đang đọc file này
│
├── 📋 ESSENTIAL GUIDES
│   ├── CHANGES.md                     ← Các fix và thay đổi đã áp dụng (April 28, 2026)
│   ├── CODEBASE_MAP.md                ← Quick reference: files, classes, methods
│   ├── CODE_STANDARDS.md              ← Code style, naming, patterns, anti-patterns
│   └── DEVELOPMENT_GUIDE.md           ← How to add features, test, debug
│
├── 🏗️ ARCHITECTURE
│   ├── architecture/system-overview.md        ← Layers, dependencies, NuGet packages
│   └── architecture/state-machine.md          ← DCR workflow state machine rules
│
├── 🚀 OPERATIONAL GUIDES  
│   ├── agent-runbooks/01-environment-setup.md      ← First-time setup
│   ├── agent-runbooks/02-build-and-verify.md       ← Build & test project
│   ├── agent-runbooks/03-run-tests.md              ← Run full test suite
│   ├── agent-runbooks/04-add-feature.md            ← Step-by-step feature development
│   └── agent-runbooks/05-ci-pipeline.md            ← CI/CD steps (future)
│
├── 🔧 SKILLS & PATTERNS
│   ├── skills/SKILL-build.md          ← dotnet build, restore, clean
│   ├── skills/SKILL-db.md             ← EF migrations, database seeding
│   ├── skills/SKILL-run.md            ← Run application locally
│   ├── skills/SKILL-test.md           ← Unit & integration testing
│   └── skills/SKILL-debug.md          ← Debugging common issues
│
├── 🧪 TESTING
│   ├── test-plans/unit-tests.md       ← Unit test scenarios
│   ├── test-plans/integration-tests.md ← Integration test scenarios
│   └── test-plans/e2e-scenarios.md    ← End-to-end UI scenarios
│
└── 🐛 MAINTENANCE
    ├── TROUBLESHOOTING.md              ← Common errors & solutions
    └── REFACTORING_OPPORTUNITIES.md   ← Future improvements & TODOs
```

---

## 🎯 Quick Navigation

### I want to...

| Goal | Read This |
|------|-----------|
| **Understand the codebase** | [CODEBASE_MAP.md](CODEBASE_MAP.md) |
| **Add a new feature** | [DEVELOPMENT_GUIDE.md](DEVELOPMENT_GUIDE.md) → [04-add-feature.md](agent-runbooks/04-add-feature.md) |
| **Fix a bug** | [TROUBLESHOOTING.md](TROUBLESHOOTING.md) |
| **Write tests** | [DEVELOPMENT_GUIDE.md](DEVELOPMENT_GUIDE.md#testing-strategy) → [unit-tests.md](test-plans/unit-tests.md) |
| **Deploy** | [02-build-and-verify.md](agent-runbooks/02-build-and-verify.md) |
| **Understand architecture** | [system-overview.md](architecture/system-overview.md) |
| **Refactor code** | [REFACTORING_OPPORTUNITIES.md](REFACTORING_OPPORTUNITIES.md) → [CODE_STANDARDS.md](CODE_STANDARDS.md) |
| **Check recent changes** | [CHANGES.md](CHANGES.md) |

---

## 🚀 Quick Start for Developers

```bash
# 1. Clone & restore
git clone <repo>
cd c:\Workbench\AppDev\C_Sharp\DCRManagement
dotnet restore DCRManagement.sln

# 2. Setup database
dotnet ef database update --project src/DCRManagement.Infrastructure --startup-project src/DCRManagement.UI

# 3. Build
dotnet build DCRManagement.sln

# 4. Run tests
dotnet test tests/DCRManagement.Tests/DCRManagement.Tests.csproj

# 5. Run app
dotnet run --project src/DCRManagement.UI/DCRManagement.UI.csproj
```

### Default Test Credentials
```
Username: admin          Password: Admin@123
Username: engineer01     Password: Engineer@123
Username: reviewer01     Password: Reviewer@123
Username: approver01     Password: Approver@123
```

---

## 📊 Project Status

| Component | Status | Last Updated |
|-----------|--------|---|
| Build | ✅ OK | April 28, 2026 |
| Unit Tests | ✅ OK | April 28, 2026 |
| Database Setup | ✅ OK | April 28, 2026 |
| Email Service | ✅ Config Fixed | April 28, 2026 |
| UI Startup | ✅ Fixed | April 28, 2026 |
| Documentation | ✅ Complete | April 28, 2026 |

---

## 🔑 Core Principles (Read These!)

1. **Result Pattern** — Services return `Result<T>` instead of throwing
   ```csharp
   if (!result.IsSuccess)
       return Result<DCRDto>.Failure(errorMsg, errorCode);
   ```

2. **MVP Pattern** — Forms have zero logic (all in Presenters)
   ```
   Form (no logic) ← Presenter (logic) → Service (business)
   ```

3. **Clean Architecture** — Strict dependency flow
   ```
   UI → Application → Domain
   Infrastructure → Domain
   ```

4. **Dependency Injection** — No `new` keyword in code
   ```csharp
   public DCRService(IDCRRepository repo, ILogger<DCRService> logger)
   {
       _repo = repo;  // Injected
   }
   ```

5. **Async All The Way** — Always use async/await
   ```csharp
   public async Task<Result<DCRDto>> GetAsync(int id)
   ```

See [CODE_STANDARDS.md](CODE_STANDARDS.md) for detailed rules.

---

## 📋 Code Quality Checks Performed

✅ No compilation errors  
✅ No syntax errors  
✅ Configuration keys match implementations  
✅ Dependency injection properly configured  
✅ Entity Framework queries validated  
✅ All tests compilable  
✅ Documentation complete  

---

## 🏗️ Architecture at a Glance

```
┌──────────────────────────────────────┐
│      DCRManagement.UI (WinForms)     │
│  Forms → Presenters → Services       │
└────────────────┬─────────────────────┘
                 │
         ┌───────┴────────┐
         ▼                ▼
┌─────────────────┐  ┌──────────────────┐
│  Application    │  │ Infrastructure   │
│  (Services)     │  │ (Persistence)    │
└────────┬────────┘  └────────┬─────────┘
         │                    │
         └────────┬───────────┘
                  ▼
         ┌─────────────────┐
         │  Domain         │
         │  (Entities,     │
         │   Interfaces)   │
         └─────────────────┘
```

See [system-overview.md](architecture/system-overview.md) for details.

---

## 📚 Latest Changes

### April 28, 2026

**Fixed Issues:**
1. ✅ **Program.cs** — Invalid Form1() reference → Fixed to LoginForm
2. ✅ **DependencyInjection** — Missing IConfiguration parameter → Added config loading
3. ✅ **appsettings.json** — Configuration key mismatch → Aligned with EmailService
4. ✅ **Documentation** — Created comprehensive guides & standards

**New Documentation:**
- [CHANGES.md](CHANGES.md) — Detailed fix log
- [CODEBASE_MAP.md](CODEBASE_MAP.md) — Quick reference
- [CODE_STANDARDS.md](CODE_STANDARDS.md) — Coding standards
- [DEVELOPMENT_GUIDE.md](DEVELOPMENT_GUIDE.md) — How to code
- [TROUBLESHOOTING.md](TROUBLESHOOTING.md) — Common errors
- [REFACTORING_OPPORTUNITIES.md](REFACTORING_OPPORTUNITIES.md) — Future improvements

See [CHANGES.md](CHANGES.md) for complete details.

---

## 🎓 Learning Path

### For New Developers

1. **Day 1:** Read [CODEBASE_MAP.md](CODEBASE_MAP.md) & [system-overview.md](architecture/system-overview.md)
2. **Day 2:** Read [CODE_STANDARDS.md](CODE_STANDARDS.md) & [DEVELOPMENT_GUIDE.md](DEVELOPMENT_GUIDE.md)
3. **Day 3:** Follow [04-add-feature.md](agent-runbooks/04-add-feature.md) to add a small feature
4. **Day 4:** Write unit tests following [unit-tests.md](test-plans/unit-tests.md)
5. **Ongoing:** Keep [TROUBLESHOOTING.md](TROUBLESHOOTING.md) handy

---

## 🤖 For Agents/Automation

This knowledge base is structured for automated systems to:

1. **Quickly find** classes, methods, patterns
2. **Understand** architecture & dependencies
3. **Modify** code following standards
4. **Test** changes thoroughly
5. **Document** changes professionally

**Key agent resources:**
- [CODEBASE_MAP.md](CODEBASE_MAP.md) — File/class locations
- [CODE_STANDARDS.md](CODE_STANDARDS.md) — Coding patterns
- [DEVELOPMENT_GUIDE.md](DEVELOPMENT_GUIDE.md) — Step-by-step guides
- [TROUBLESHOOTING.md](TROUBLESHOOTING.md) — Error solutions

---

## 📞 Support & Resources

- **Architecture Questions** → [system-overview.md](architecture/system-overview.md)
- **Coding Issues** → [CODE_STANDARDS.md](CODE_STANDARDS.md)
- **How-to Questions** → [DEVELOPMENT_GUIDE.md](DEVELOPMENT_GUIDE.md)
- **Errors & Problems** → [TROUBLESHOOTING.md](TROUBLESHOOTING.md)
- **Future Work** → [REFACTORING_OPPORTUNITIES.md](REFACTORING_OPPORTUNITIES.md)
- **Code Locations** → [CODEBASE_MAP.md](CODEBASE_MAP.md)

---

## ✅ Quality Assurance Checklist

Before committing code:

- [ ] Code follows [CODE_STANDARDS.md](CODE_STANDARDS.md)
- [ ] Uses `Result<T>` pattern (never throws for business logic)
- [ ] No logic in UI forms (all in presenters)
- [ ] All dependencies injected (no `new` keyword)
- [ ] Async/await used everywhere
- [ ] Tests written & passing
- [ ] No N+1 queries (use `.Include()`)
- [ ] Structured logging (not string interpolation)
- [ ] XML comments on public APIs
- [ ] Related docs updated

---

**Last Updated:** April 28, 2026  
**Status:** ✅ Production Ready  
**Documentation Version:** 2.0  

