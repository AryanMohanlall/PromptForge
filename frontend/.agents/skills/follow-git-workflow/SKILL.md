---
name: follow-git-workflow
description: Use this skill before starting ANY feature or fix. Covers branch naming, commit message conventions, the mandatory planning step, and the push workflow. The agent must always show a plan and create a branch before writing code.
---

# GovLeave — Git Workflow

## The Rule

**No code is written without a branch. No branch is pushed without a plan.**

Every piece of work — no matter how small — follows this sequence:

```
1. Show plan
2. Create branch
3. Write code
4. Commit
5. Push
```

---

## Branch Naming

```bash
# New features
git checkout -b feature/<descriptive-name>

# Bug fixes
git checkout -b fix/<descriptive-name>

# Refactoring
git checkout -b refactor/<descriptive-name>

# Chores (deps, config, tooling)
git checkout -b chore/<descriptive-name>
```

### Examples
```bash
feature/auth-provider
feature/employee-table
feature/leave-approval-modal
feature/department-crud
fix/axios-401-interceptor
fix/leave-form-validation
refactor/extract-status-badge
chore/install-antd-style
```

---

## Starting a Branch

Always branch from `main`:

```bash
git checkout main
git pull origin main
git checkout -b feature/<name>
```

---

## Commit Message Format

```
<type>: <short description>
```

| Type | When to use |
|---|---|
| `feat` | New feature or provider |
| `fix` | Bug fix |
| `refactor` | Code change, no behaviour change |
| `style` | Styling only |
| `chore` | Deps, config, tooling |
| `docs` | README, comments |

### Examples
```bash
git commit -m "feat: add employee provider with CRUD actions"
git commit -m "feat: add leave approval modal with manager comment"
git commit -m "fix: correct ABP result unwrapping in department provider"
git commit -m "fix: redirect to login on 401 response"
git commit -m "style: add styles folder for employees page"
git commit -m "chore: install js-cookie and antd-style"
```

---

## Push

```bash
git push origin feature/<name>
```

---

## Mandatory Planning Step

Before writing a single line of code, the agent MUST output a plan in this exact format:

```
## Plan: <feature name>

**Branch:** feature/<name>

**Skills to read:**
- govleave-api (if making API calls)
- govleave-provider-pattern (if building a provider)
- govleave-styling (if building a page/component)
- govleave-auth-provider (if touching auth)

**Steps:**
1. Install dependencies (if any): <list packages>
2. Create files:
   - src/providers/employee-provider/context.tsx
   - src/providers/employee-provider/actions.tsx
   - src/providers/employee-provider/reducer.tsx
   - src/providers/employee-provider/index.tsx
3. Modify files:
   - src/providers/index.tsx (add EmployeeProvider)
4. Test: visit /employees and verify table loads

**ABP endpoints used:**
- GET /api/services/app/Employee/GetAll
- POST /api/services/app/Employee/Create
- PUT /api/services/app/Employee/Update
- DELETE /api/services/app/Employee/Delete

**Potential issues:**
- Employee IDs are UUIDs (string), not numbers
- Department must be loaded first (departmentId FK)
```

Only after the plan is shown and confirmed should the agent proceed.

---

## Full Workflow Example

```bash
# 1. Start clean
git checkout main
git pull origin main

# 2. Create branch
git checkout -b feature/employee-provider

# 3. ... write code ...

# 4. Commit
git add .
git commit -m "feat: add employee provider with full CRUD"

# 5. Push
git push origin feature/employee-provider
```

