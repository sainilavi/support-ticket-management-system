# Reflection

**Project:** Support Ticket Management System  
**Stack:** ASP.NET Core 8 · Clean Architecture · EF Core · SQL Server  
**Development approach:** Iterative, AI-assisted, human-reviewed

---

## 1. What Was Built

A full backend REST API for managing support tickets, developed in nine incremental milestones over a structured prompt-driven workflow.

### Deliverables

| Category | Output |
|----------|--------|
| **API** | Ticket CRUD, search/filter/pagination, comment APIs, Swagger documentation |
| **Architecture** | Four-layer Clean Architecture solution (Domain, Application, Infrastructure, API) |
| **Data** | `User`, `Ticket`, `Comment` entities; EF Core migrations; idempotent seed data |
| **Business rules** | Status workflow (`Open` → `InProgress` → `Resolved` → `Closed`, with `Cancelled` branch) |
| **Cross-cutting** | FluentValidation, global exception middleware, consistent `ApiResponse` / `ApiErrorResponse` envelopes |
| **Tests** | 14 passing integration tests (xUnit, `WebApplicationFactory`, InMemory DB) |
| **Documentation** | README, requirements analysis, implementation plan, design notes, test strategy, debugging notes, code review notes, tool workflow |

### By the Numbers

| Metric | Value |
|--------|-------|
| Solution projects | 4 source + 1 test |
| API endpoints | 7 |
| Domain entities | 3 |
| EF Core migrations | 2 |
| Integration tests | 14 (all passing) |
| Issues found and fixed | 28 |
| Architecture improvements | 28 |

---

## 2. How AI Was Used

Cursor AI (Agent mode) was the primary development assistant. Work was organized as **focused, sequential prompts** — not a single bulk generation.

### Development Flow

```
Scaffold → Entities/DB → CRUD → Comments → Workflow → Search →
Validation → Exception handling → Swagger → Tests → Seed data →
Architecture review → Bug fixes → Documentation
```

### AI Capabilities Leveraged

| Capability | How It Was Used |
|------------|-----------------|
| **Code generation** | Full vertical slices across all four layers per feature |
| **Codebase exploration** | Read existing files before each change to follow conventions |
| **Shell execution** | `dotnet build`, `dotnet test`, `dotnet ef migrations` |
| **Refactoring** | Architecture review pass — removed unused abstractions, optimized queries |
| **Debugging** | Diagnosed build errors, null references, HTTP status inconsistencies |
| **Documentation** | Generated README, design notes, test strategy, and supporting docs |
| **Subagent exploration** | Broad backend audit during code review and bug analysis |

### Prompt Strategy

- **One feature per prompt** — e.g., "Implement Ticket CRUD" not "build everything"
- **Explicit constraints** — e.g., "structure only", "do not change business logic"
- **Review passes** — separate prompts for architecture review, bug fixing, and documentation
- **Verify after each step** — build and test before moving on

> See [tool-workflow.md](./tool-workflow.md) for the full AI workflow documentation.

---

## 3. AI Strengths

### What AI Did Well

| Strength | Example |
|----------|---------|
| **Rapid scaffolding** | Full Clean Architecture solution with DI, middleware, and repository pattern in one session |
| **Consistent patterns** | Every feature followed the same DTO → Validator → Service → Repository → Controller flow |
| **Breadth of coverage** | Single prompt produced entities, EF configs, migration, validators, services, controllers, and Swagger annotations |
| **Refactoring at scale** | Architecture review touched 20+ files — removed dead code, consolidated duplicates, optimized queries |
| **Documentation** | Generated professional README, requirements analysis, and design docs from codebase context |
| **Debugging speed** | Identified root causes (NRE in validator, 400 vs 404 mismatch, `IHost` in wrong layer) and applied targeted fixes |
| **Convention adherence** | Matched naming, folder structure, and error-handling patterns across all generated code |
| **Test generation** | Created integration test project with theory-driven status transition tests and BFS path builder |

### Where AI Added the Most Value

1. **Boilerplate elimination** — DI registration, EF configurations, AutoMapper profiles, and validator wiring
2. **Cross-layer changes** — Features that touch Domain through API in a single coherent pass
3. **Review and hardening** — Second-pass analysis found real bugs (null guards, pagination overflow, error shape inconsistency)
4. **Documentation at speed** — Seven supporting documents generated from actual project state

---

## 4. AI Mistakes

AI was effective but not infallible. The following mistakes required correction or human intervention.

### Build & Compilation Errors

| Mistake | Impact | Resolution |
|---------|--------|------------|
| Used `&&` in PowerShell commands | Commands failed on Windows | Switched to `;` separator |
| Left `Repository<T>` implementing deleted `IRepository<T>` | Build break after refactor | Made base class standalone abstract |
| Placed `HostDatabaseExtensions` in Infrastructure | `IHost` not available; violated Clean Architecture | Moved to API layer |
| Referenced `Validation.ValidationMessages` with wrong namespace | Compilation error | Fixed using directive |
| Attempted `ForAllMaps` on AutoMapper 12 | API not available in that version | Reverted; documented upgrade path |

### Design Mistakes

| Mistake | Impact | Resolution |
|---------|--------|------------|
| Generic `IRepository<T>` registered in DI but never used | Unnecessary abstraction | Removed interface and DI registration |
| `UnitOfWork` disposed `DbContext` | Double-dispose risk with DI | Removed `IDisposable` from `IUnitOfWork` |
| `TicketSearchFilter` duplicated `TicketQueryDto` | Two models for same concept | Consolidated to `TicketQueryDto` |
| Duplicate `FormatUserName` in AutoMapper profiles | DRY violation | Extracted `UserDisplayNameFormatter` |
| Architecture refactor left build in broken state mid-session | Incomplete dependent file updates | Completed in follow-up pass |

### Logic & Behavior Mistakes

| Mistake | Impact | Resolution |
|---------|--------|------------|
| Status transition validator dereferenced null `Dto` | Potential 500 on null body | Added `.When(x => x.Dto is not null)` guard |
| Missing ticket on comments returned 400, not 404 | Inconsistent HTTP semantics | `NotFoundException` in service layer |
| `null` ticket status passed closed-ticket validator | Race edge case allowed | Fail when `status is null` |
| `AssignedToUserId = 0` showed role error | Misleading validation message | Check `> 0` before role lookup |
| Model-binding errors used different shape than FluentValidation | Inconsistent API contract | `ApiBehaviorExtensions` unified envelope |

### Process Mistakes

| Mistake | Impact | Resolution |
|---------|--------|------------|
| Started architecture refactor without finishing all dependent files | Temporary build failure | Completed in next turn |
| Did not flag authentication gap until explicit security review | Security risk undocumented initially | Documented as P0 in review notes |
| Comment APIs have no integration tests | Coverage gap | Documented in test strategy roadmap |

### Pattern Observed

AI excels at **generating correct-looking code** but can miss **edge cases**, **layer boundary violations**, and **incomplete refactors** when changes span many files. Build and test verification after every step was essential to catch these.

---

## 5. Manual Validation

All AI-generated output was subject to human review before acceptance.

### Review Process

```
AI generates code/docs
    → Human reviews diff (logic, conventions, scope)
    → dotnet build (compilation gate)
    → dotnet test (regression gate)
    → Accept, revise, or reject
```

### What Was Manually Verified

| Check | Method |
|-------|--------|
| **Compilation** | `dotnet build` after every change — zero errors required |
| **Regression** | `dotnet test` — 14/14 integration tests must pass |
| **Business logic** | Status workflow rules match requirements; no unintended changes during bug fixes |
| **Architecture** | Dependency direction, no Infrastructure → API leaks |
| **Scope control** | Rejected over-engineering (e.g., auth not implemented when only review was requested) |
| **Security** | Flagged missing authentication; no secrets in code or config |
| **Documentation accuracy** | Docs cross-referenced against actual codebase state |

### Validation Outcomes

| Outcome | Count |
|---------|-------|
| Changes accepted as-is | Majority of feature implementations |
| Changes revised before acceptance | Validation error shape, HTTP status codes, naming |
| Changes rejected or reverted | AutoMapper `ForAllMaps` (wrong version), incomplete refactor artifacts |
| Issues found only during manual review | 404 vs 400 inconsistency, pagination overflow, null `Dto` NRE |

**Key takeaway:** AI accelerated development significantly, but **human review and automated tests were the quality gate**. Neither alone would have been sufficient.

---

## 6. Lessons Learned

### On AI-Assisted Development

1. **Small prompts win.** One feature per prompt produced reviewable, testable increments. Broad prompts risk incomplete or over-scoped output.

2. **Build and test are non-negotiable.** Every AI change should be followed by `dotnet build` and `dotnet test`. This caught ~40% of AI mistakes before they compounded.

3. **Explicit constraints matter.** Phrases like "structure only", "do not change business logic", and "fix automatically" dramatically changed AI behavior and output quality.

4. **Review passes are worth separate prompts.** The architecture review and bug-fix passes found real issues that feature implementation prompts missed.

5. **AI repeats itself.** Duplicate helpers, redundant abstractions, and copy-pasted patterns emerged naturally. A dedicated review pass for DRY violations is valuable.

6. **AI does not self-verify security.** Input validation was implemented thoroughly, but authentication was not flagged until explicitly requested. Security must be a named requirement.

### On Clean Architecture

7. **Keep interfaces lean.** The generic `IRepository<T>` seemed like good practice but violated ISP and was never used. Entity-specific interfaces are better.

8. **Respect layer boundaries.** Placing `HostDatabaseExtensions` in Infrastructure seemed convenient but introduced a forbidden dependency. The API layer is the right place for hosting concerns.

9. **Domain rules belong in Domain.** Moving status transitions to `TicketStatusWorkflow` made validators and services simpler and the rules testable in isolation.

### On API Design

10. **Unify error shapes early.** Having FluentValidation and model-binding return different error formats created client confusion. One `ApiErrorResponse` envelope everywhere is worth the upfront investment.

11. **HTTP semantics matter.** 404 vs 400 for missing resources, 204 for DELETE, 409 for conflicts — these details were wrong initially and only caught during review.

12. **Validation ordering affects status codes.** Checking ticket existence before business rules ensures 404 instead of misleading 400 messages.

---

## 7. Future Improvements

### High Priority

| Item | Rationale |
|------|-----------|
| **JWT authentication & authorization** | API is fully public; `CreatedByUserId` / `UserId` in request body is an IDOR risk |
| **Comment API integration tests** | Only ticket endpoints are tested; comments have zero coverage |
| **Validation integration tests** | Required fields, max lengths, user role errors untested |
| **Derive user identity from claims** | Remove user IDs from request bodies once auth exists |

### Medium Priority

| Item | Rationale |
|------|-----------|
| **AutoMapper upgrade to 15.1.3+** | Clears NU1903 security advisory |
| **Comment pagination** | All comments loaded for a ticket; won't scale |
| **Unit tests for validators and domain** | Faster feedback than full integration tests |
| **Restrict `AllowedHosts`** | Production hardening |
| **Rate limiting** | Protect public endpoints |

### Low Priority

| Item | Rationale |
|------|-----------|
| **Move `ApiResponse<T>` to API layer** | Presentation concern in Application |
| **SQL Server smoke tests in CI** | InMemory provider doesn't catch all FK behaviors |
| **Health check endpoints** | Operational observability |
| **Soft delete for tickets** | Preserve audit history |
| **Email notifications** | Notify customers on status changes |

### On AI Workflow

| Item | Rationale |
|------|-----------|
| **Add AI-generated tests for every new endpoint** | Prompt for tests alongside features |
| **Run security review as a standard milestone** | Don't wait for explicit request |
| **Pin dependency versions in review prompts** | Avoid API mismatches (AutoMapper `ForAllMaps`) |
| **Use Bugbot/Security Review subagents** | Automated review before human acceptance |

---

## Closing Thought

This project demonstrates that AI-assisted development can produce a **well-structured, tested, and documented** backend API in a fraction of the time of traditional solo development — provided the workflow includes **iterative prompts, automated verification, and human review**.

The AI handled breadth (scaffolding, features, docs, refactoring) exceptionally well. Humans (or human-directed review) handled depth (edge cases, security gaps, HTTP semantics, incomplete refactors). The combination was more effective than either alone.

---

## Related Documents

- [tool-workflow.md](./tool-workflow.md) — How Cursor AI was used
- [code-review-notes.md](./code-review-notes.md) — SOLID, architecture, and security review
- [debugging-notes.md](./debugging-notes.md) — Problems and fixes
- [test-strategy.md](./test-strategy.md) — Testing approach and gaps
- [README.md](./README.md) — Project overview and setup
