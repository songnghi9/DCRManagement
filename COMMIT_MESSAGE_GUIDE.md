# Commit Message Guide

## Format

```
<type>(<scope>): <subject>

<body>

<footer>
```

---

## Type

Must be one of:

- **feat** — A new feature
- **fix** — A bug fix
- **docs** — Documentation only
- **refactor** — Code refactor without feature/fix
- **perf** — Performance improvement
- **test** — Adding or updating tests
- **chore** — Build, dependencies, or tooling
- **style** — Code style only (formatting, missing semicolons, etc)

## Scope

Optional. Area affected:

- `api` — API changes
- `auth` — Authentication/Authorization
- `dcr` — DCR operations
- `workflow` — Workflow state machine
- `email` — Email notifications
- `db` — Database/EF Core
- `ui` — UI forms
- `tests` — Test suite
- `docs` — Documentation

## Subject

- Imperative mood ("add", not "added")
- No period at end
- Max 50 characters
- Lowercase first letter (except proper nouns)

## Body

- Explain *what* and *why*, not *how*
- Wrap at 72 characters
- Separate from subject with blank line
- Reference issues with `Closes #123`

## Footer

- Reference related issues: `Closes #123`
- Breaking changes: `BREAKING CHANGE: description`

---

## Examples

### Feature

```
feat(workflow): add approval comment feature

Allow reviewers to add comments when approving/rejecting DCRs.
Comments are displayed in workflow history.

Closes #42
```

### Bug Fix

```
fix(dcr): prevent duplicate DCR submission

SessionContext was not cleared after logout, causing
stale user context to be used on next login.

Closes #55
```

### Refactor

```
refactor(db): extract validation to FluentValidation

- Created CreateDCRValidator, UpdateDCRValidator
- Extracted validation logic from DCRService
- Enables reuse in API layer when added

No functional changes.
```

### Documentation

```
docs: update CODE_STANDARDS with MVP pattern examples

Added clarification on MVP pattern with code examples
for proper separation of concerns in UI layer.
```

### Performance

```
perf(dcr): add query optimization with explicit Includes

Previously queries missing .Include() caused N+1 queries.
Now GetWithFullDetailsAsync explicitly loads related data.

Closes #88
```

### Configuration

```
chore: update NuGet packages to latest versions

- MailKit 4.17.0 (was 4.16.0)
- EntityFrameworkCore 8.0.1 (was 8.0.0)
- Moq 4.16.1 (was 4.16.0)
```

---

## Tips

✅ Use imperative mood ("add feature" not "added feature")  
✅ Be specific and concise  
✅ Reference issues  
✅ Explain the "why"  
✅ One logical change per commit  

❌ Don't use vague messages ("fix stuff", "updates")  
❌ Don't mix unrelated changes  
❌ Don't commit broken code  
❌ Don't ignore failing tests  

---

## Commit Checklist

Before committing:

```bash
# 1. Build
dotnet build DCRManagement.sln

# 2. Tests pass
dotnet test tests/DCRManagement.Tests/DCRManagement.Tests.csproj

# 3. Code follows standards
# (manual check or linter)

# 4. Commit with message
git add .
git commit -m "feat(dcr): add approval comment feature

Allow reviewers to add comments when approving/rejecting DCRs.
Comments are displayed in workflow history.

Closes #42"

# 5. Push
git push origin feature-branch
```

---

## Tools

### Setup git message template (optional)

```bash
# Create .gitmessage file
cat > .gitmessage << 'EOF'
# <type>(<scope>): <subject>
#
# <body>
#
# <footer>

# Allowed types: feat, fix, docs, refactor, perf, test, chore, style
# Allowed scopes: api, auth, dcr, workflow, email, db, ui, tests, docs
EOF

# Configure git to use it
git config commit.template .gitmessage
```

---

See [CODE_STANDARDS.md](docs/CODE_STANDARDS.md) for more development standards.
