# Pull Request Template

## 📋 Description

<!-- Clear, concise description of what this PR does -->
Brief summary of changes...

## 🎯 Type of Change

- [ ] 🐛 Bug fix (non-breaking change that fixes an issue)
- [ ] ✨ New feature (non-breaking change that adds functionality)
- [ ] 📚 Documentation update
- [ ] ♻️ Refactor (no functional change)
- [ ] 🔧 Configuration/Dependencies update
- [ ] 🧪 Test additions/improvements

## 📝 Changes Made

<!-- List specific changes -->
- [ ] Change 1
- [ ] Change 2
- [ ] Change 3

## ✅ Testing Done

- [ ] Unit tests added/updated
- [ ] Manual testing completed
- [ ] All tests passing: `dotnet test`
- [ ] No new compiler warnings

## 🔗 Related Issues

Closes #123 (if applicable)

## 📋 Checklist

- [ ] Code follows [CODE_STANDARDS.md](../../docs/CODE_STANDARDS.md)
- [ ] Services use `Result<T>` pattern (not exceptions)
- [ ] No logic in UI forms (all in presenters)
- [ ] All dependencies injected (no `new` keyword)
- [ ] Async/await used correctly
- [ ] No N+1 queries (proper `.Include()` usage)
- [ ] Structured logging used (not string interpolation)
- [ ] XML comments added for public APIs
- [ ] Tests written for new functionality
- [ ] Documentation updated (if needed)

## 🎨 Code Quality

```bash
# Verify before submitting:
dotnet build DCRManagement.sln --configuration Debug
dotnet test tests/DCRManagement.Tests/DCRManagement.Tests.csproj
```

## 📸 Screenshots (if applicable)

<!-- Add before/after screenshots for UI changes -->

## 🚀 Deployment Notes (if applicable)

<!-- Any migration scripts, configuration changes, or deployment instructions -->
- Database migration required: Yes/No
- Configuration changes: None/List changes
- Breaking changes: Yes/No

## 👥 Reviewers

<!-- Mention who should review -->
@...

---

**Note:** This PR template is based on [DEVELOPMENT_GUIDE.md](../../docs/DEVELOPMENT_GUIDE.md) standards.
