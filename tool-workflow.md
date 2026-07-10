# Cursor AI Tool Workflow

This document describes how **Cursor AI** was used to build the **Support Ticket Management System** backend. The project was developed iteratively through structured prompts, with each phase building on the previous one.

> **Important:** All AI-generated code, configuration, and documentation were **manually reviewed before acceptance**. Suggestions were evaluated against project requirements, existing conventions, and build/test results. Changes were accepted, revised, or rejected based on human judgment.

---

## Overview

Cursor AI acted as an interactive development assistant throughout the lifecycle of the project—from initial scaffolding to architecture review, bug fixing, and documentation. Work was organized as a series of focused prompts rather than a single monolithic generation, which kept scope manageable and allowed verification at each step.

```
Requirements → Planning → Architecture → Implementation → Validation → Testing → Review → Documentation
```

---

## 1. Requirement Analysis

Cursor AI was used to interpret natural-language requirements and translate them into actionable implementation tasks.

**Examples from this project:**

| Prompt | Outcome |
|--------|---------|
| Build ASP.NET Core 8 Web API with Clean Architecture (structure only) | Scoped to scaffolding; features explicitly deferred |
| Implement Ticket CRUD with DTOs, repository, service, controller | Full vertical slice identified across four layers |
| Implement ticket status workflow with allowed transitions | Business rules extracted into a domain workflow definition |
| Add search, filter, and pagination | Query parameters and `PagedResult<T>` requirements clarified |
| Add backend validation with FluentValidation | Field-level and cross-field rules enumerated |

The agent read existing code before each task to avoid conflicting with prior decisions and to follow established naming and folder conventions.

---

## 2. Planning

Before writing code, Cursor AI broke larger requests into ordered steps and tracked progress with task lists.

**Typical planning activities:**

- Inspecting the workspace (empty vs. existing structure)
- Identifying which layers each feature touches (Domain → Application → Infrastructure → API)
- Sequencing work: entities before migrations, services before controllers, validators alongside DTOs
- Deferring out-of-scope items (e.g., authentication flagged during security review but not implemented)

For example, the initial request specified **“project structure only”**, and the agent planned accordingly—creating solution layout, DI wiring, and skeleton files without implementing business features until a follow-up prompt.

---

## 3. Architecture

Cursor AI designed and maintained a **Clean Architecture** layout with strict dependency rules.

### Layer responsibilities

| Layer | Responsibility |
|-------|----------------|
| **Domain** | Entities, enums, workflow rules, domain exceptions |
| **Application** | DTOs, service interfaces, validators, AutoMapper profiles, application exceptions |
| **Infrastructure** | EF Core DbContext, configurations, repositories, migrations, seed data |
| **API** | Controllers, middleware, Swagger, startup and DI composition |

### Patterns applied

- **Repository + Unit of Work** — Data access abstracted behind interfaces in Application, implemented in Infrastructure
- **Service layer** — Business orchestration between controllers and repositories
- **Dependency Injection** — Layer-specific `DependencyInjection` extension methods
- **Global exception middleware** — Centralized error mapping and logging at the API boundary

During a later architecture review, Cursor AI refactored unused abstractions (e.g., generic `IRepository<T>`, marker `IService`), consolidated duplicate helpers, and optimized repository read/write paths—while preserving the overall structure.

---

## 4. Code Generation

Cursor AI generated code incrementally across all layers. Generation was **prompt-driven and feature-scoped**, not bulk-dumped.

### Phase 1 — Foundation
- Solution and four-project structure
- `ApplicationDbContext`, base repository, unit of work
- Global exception middleware skeleton
- Swagger and DI registration

### Phase 2 — Domain & data
- `User`, `Ticket`, `Comment` entities and enums
- EF Core fluent configurations and relationships
- Initial migration (`InitialCreate`)
- Runtime seed data (users, tickets, comments)

### Phase 3 — Features
- Ticket CRUD (DTOs, repository, service, controller, validators, AutoMapper)
- Comment APIs (add comment, list by ticket)
- Status workflow enforcement (`TicketStatusWorkflow`)
- Search, filter, and pagination (`TicketQueryDto`, `PagedResult<T>`)

### Phase 4 — Cross-cutting concerns
- FluentValidation rules and consistent `ApiErrorResponse` shape
- Swagger XML comments, grouped endpoints, sample requests/responses
- `dotnet-tools.json` for EF Core CLI
- Connection string moved to `appsettings.json` (not hardcoded in `Program.cs`)

### Phase 5 — Quality & hardening
- Architecture review fixes (SOLID, performance, naming)
- Bug and edge-case fixes (null guards, HTTP status consistency, model-binding errors)
- `README.md` and project documentation

Each generation pass was followed by `dotnet build` to confirm compilation.

---

## 5. Validation

Cursor AI implemented and refined input validation using **FluentValidation**.

**What was generated:**

- Per-DTO validators (`CreateTicketDto`, `UpdateTicketDto`, `CreateCommentDto`, `TicketQueryDto`)
- Cross-request validators (`UpdateTicketRequest`, `CreateCommentRequest`)
- Shared rule extensions (`ValidTitle`, `ValidAssignedUser`, `ValidTicketId`)
- Centralized validation messages and constants
- `ValidationExtensions` to throw `ValidationException` with camelCase field keys
- `ApiBehaviorExtensions` to align model-binding errors with the same `ApiErrorResponse` envelope

**Validation coverage includes:**

- Required fields and max lengths (title, description, comment content)
- Enum validity (priority, status)
- User existence and role checks (customer, agent/admin)
- Ticket existence and status transition rules
- Pagination bounds (page number, page size)
- Closed/cancelled ticket comment restrictions

---

## 6. Testing

Cursor AI created an **integration test project** using xUnit and `WebApplicationFactory`.

### Test project setup
- `SupportTicketManagementSystem.IntegrationTests`
- In-memory EF Core database per test run
- `CustomWebApplicationFactory` with `Testing` environment
- Shared HTTP helpers for JSON requests/responses

### Test coverage (14 tests)
- Create ticket
- Update ticket
- Valid status transitions (theory-driven)
- Invalid status transitions (theory-driven)
- Search with keyword, status filter, and pagination

Tests were run after major changes:

```bash
dotnet test tests/SupportTicketManagementSystem.IntegrationTests
```

Cursor AI used failing builds and test output to verify fixes during debugging and review cycles.

---

## 7. Debugging

Cursor AI was used to diagnose and resolve runtime and compile-time issues.

**Examples:**

| Issue | Resolution |
|-------|------------|
| PowerShell `&&` not supported | Switched to `;` separator in shell commands |
| EF Core CLI not found | Added `dotnet-tools.json` and `Microsoft.EntityFrameworkCore.Design` to API project |
| Connection string in `Program.cs` | Moved to `appsettings.json`; read via `AddInfrastructure(configuration)` |
| Build break after removing `IRepository<T>` | Updated base `Repository<T>` to standalone abstract class |
| `IHost` not available in Infrastructure | Moved `HostDatabaseExtensions` to API layer |
| `NullReferenceException` on null `UpdateTicketDto` | Added `.When(x => x.Dto is not null)` guard in validator |
| Missing ticket returned 400 on comment endpoints | Aligned with 404 via `NotFoundException` in service layer |

The agent re-ran builds and tests after each fix to confirm resolution before moving on.

---

## 8. Code Review

Cursor AI performed structured backend reviews on request, covering:

- **SOLID principles** — Interface segregation, single responsibility
- **Clean Architecture** — Dependency direction, layer boundaries
- **Repository pattern** — Slim interfaces, read vs. write query separation
- **Service layer** — Reduced redundant database round-trips
- **Duplicate code** — Shared formatters and normalizers extracted
- **Naming** — Clearer method names (`GetByIdForUpdateAsync`)
- **Performance** — `AsNoTracking()`, post-save navigation loading
- **Security** — Auth gaps identified (documented, not silently ignored)

A separate pass targeted **bugs and edge cases** without changing business logic:

- Null reference guards
- HTTP status code consistency (404 vs 400, 204 for DELETE, 409 for DB conflicts)
- Validation ordering (existence checks before business rules)
- Pagination overflow protection

Review findings were categorized by severity, fixed in code, and verified with build + test runs.

---

## Workflow Summary

| Phase | Cursor AI Role | Human Role |
|-------|----------------|------------|
| Requirement analysis | Parsed prompts, scoped tasks, read existing code | Defined requirements, approved scope |
| Planning | Broke work into steps, sequenced layers | Confirmed priorities and constraints |
| Architecture | Designed layers, patterns, and refactors | Validated architectural decisions |
| Code generation | Wrote code across all projects | **Manually reviewed all generated code** |
| Validation | Implemented FluentValidation and error handling | Verified rules match business needs |
| Testing | Created integration tests, ran test suite | Reviewed coverage and assertions |
| Debugging | Diagnosed errors, applied targeted fixes | Accepted or rejected proposed fixes |
| Code review | Audited SOLID, security, bugs; applied fixes | Final approval before merge/acceptance |

---

## Manual Review Policy

All output from Cursor AI—source files, migrations, configuration, tests, and documentation—was subject to **manual review before acceptance**. This included:

1. Reading generated code for correctness and consistency with project conventions
2. Verifying that business rules were not altered unintentionally
3. Running `dotnet build` and `dotnet test` to confirm the solution compiles and passes
4. Rejecting or revising changes that introduced unnecessary scope, insecure patterns, or architectural violations

Cursor AI accelerated development and review, but **human oversight remained the final gate** for every change integrated into the project.

---

## Tooling Used Within Cursor

| Capability | Usage |
|------------|--------|
| **Agent mode** | Multi-file code generation, shell commands, iterative fixes |
| **Codebase search** | Finding references, conventions, and impacted files |
| **Terminal** | `dotnet build`, `dotnet test`, `dotnet ef migrations` |
| **Explore subagent** | Broad backend audit during architecture and bug review |
| **Conversation context** | Continuity across phased prompts in a single session |

---

## Related Documentation

- [README.md](./README.md) — Project overview, setup, and API reference
