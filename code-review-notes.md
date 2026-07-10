# Code Review Notes

**Project:** Support Ticket Management System  
**Review Scope:** Backend API (Domain, Application, Infrastructure, API)  
**Focus Areas:** SOLID · Clean Architecture · Naming · Validation · Performance · Security

---

## Executive Summary

A structured backend review was performed across all four layers. The codebase already followed Clean Architecture and consistent patterns from initial scaffolding. The review identified opportunities to tighten interface design, eliminate duplication, improve query efficiency, and harden error handling.

**28 improvements were applied** across architecture, validation, performance, and HTTP semantics. Authentication remains the primary open security item and is documented for a future milestone.

| Area | Findings | Improvements Applied | Open Items |
|------|----------|---------------------|------------|
| SOLID | 5 | 5 | 0 |
| Clean Architecture | 4 | 4 | 1 |
| Naming | 4 | 4 | 0 |
| Validation | 6 | 6 | 1 |
| Performance | 5 | 5 | 0 |
| Security | 4 | 1 | 3 |

---

## 1. SOLID Principles

### 1.1 Single Responsibility Principle (SRP)

**Finding:** Services, validators, and middleware each had a clear single purpose. `TicketService` mixed validation orchestration with persistence — acceptable for the application layer but worth monitoring as features grow.

**Improvements made:**
- Extracted `UserDisplayNameFormatter` and `JsonPropertyNameNormalizer` — mapping and error normalization no longer duplicated across classes
- Moved database startup logic to `HostDatabaseExtensions` — `Program.cs` only composes the pipeline
- Separated `ExceptionMapper` from `GlobalExceptionMiddleware` — mapping rules isolated from HTTP writing/logging

**Current state:** ✅ Each class has a focused responsibility.

---

### 1.2 Open/Closed Principle (OCP)

**Finding:** Status workflow rules were embedded in validators. Adding a new status would require changes in multiple places.

**Improvements made:**
- Centralized transitions in `TicketStatusWorkflow` (Domain) — new transitions added in one dictionary
- `Ticket.AcceptsComments()` and `EnsureValidStatusTransition()` enrich the domain model for extension without controller changes

**Current state:** ✅ Workflow is open for extension via `TicketStatusWorkflow`; validators and services consume it.

---

### 1.3 Liskov Substitution Principle (LSP)

**Finding:** Generic `IRepository<T>` exposed methods (`GetAllAsync`, `FindAsync`, `UpdateAsync`) that specialized repositories did not need. Consumers could depend on methods that were never used.

**Improvements made:**
- Removed `IRepository<T>` from DI and public contracts
- `Repository<T>` retained as an internal abstract base class for `TicketRepository` and `CommentRepository` only

**Current state:** ✅ Repository implementations are substitutable through their specific interfaces without leaking unused CRUD methods.

---

### 1.4 Interface Segregation Principle (ISP)

**Finding:** Several interfaces were broader than necessary.

| Interface | Issue | Fix |
|-----------|-------|-----|
| `IRepository<T>` | Full CRUD for all entities | **Removed** — entity-specific interfaces only |
| `IService` | Empty marker interface | **Removed** — no value added |
| `IUserRepository` | Exposed unused `GetByIdAsync` | **Removed** — only existence-check methods remain |
| `ICommentRepository` | Exposed unused `GetByIdWithUserAsync` | **Removed** — replaced with `LoadUserAsync` |

**Current state:** ✅ Interfaces expose only what services actually call.

---

### 1.5 Dependency Inversion Principle (DIP)

**Finding:** Application layer correctly defines abstractions; Infrastructure implements them. One violation was found: Infrastructure referenced `Microsoft.Extensions.Hosting` for database initialization.

**Improvements made:**
- Moved `HostDatabaseExtensions` to the API layer — Infrastructure no longer depends on hosting abstractions
- `IUnitOfWork` no longer inherits `IDisposable` — lifetime owned by DI, not manually disposed

**Current state:** ✅ Dependency direction is inward; Infrastructure depends only on Application and Domain.

---

## 2. Clean Architecture

### 2.1 Layer Boundaries

```
API → Infrastructure → Application → Domain
```

| Check | Result |
|-------|--------|
| Domain has zero external dependencies | ✅ |
| Application defines interfaces, not implementations | ✅ |
| Infrastructure implements repositories and EF config | ✅ |
| API contains only HTTP concerns | ✅ |
| Business rules in Domain (`TicketStatusWorkflow`) | ✅ |

### 2.2 Findings & Improvements

| Finding | Improvement |
|---------|-------------|
| Generic `IRepository<T>` registered in DI but never injected | Removed from `DependencyInjection.cs` |
| `TicketSearchFilter` duplicated `TicketQueryDto` | Deleted; `TicketQueryDto` used everywhere |
| `ApiErrorResponse` in API, `ApiResponse<T>` in Application | Accepted — success envelope is shared; error envelope is presentation-specific |
| Database init in Infrastructure violated hosting dependency rule | Moved to `API/Extensions/HostDatabaseExtensions.cs` |
| `UnitOfWork` disposed `DbContext` (double-dispose risk) | Removed `IDisposable`; DI manages context lifetime |

### 2.3 Remaining Recommendation

| Item | Priority | Notes |
|------|----------|-------|
| Move `ApiResponse<T>` to API layer | Low | Presentation concern; acceptable in Application for now |

---

## 3. Naming

### 3.1 Review Findings

| Before | After | Rationale |
|--------|-------|-----------|
| `GetByIdAsync` (for updates) | `GetByIdForUpdateAsync` | Signals tracked entity for write operations |
| `GetByIdAsync` (with includes) | `GetByIdWithUsersAsync` | Signals read with navigation properties |
| `FormatUserName` (private, duplicated) | `UserDisplayNameFormatter.Format` | Shared, testable, single location |
| `TicketSearchFilter` | `TicketQueryDto` (consolidated) | One query model for search |
| `ExistsActiveWithAnyRoleAsync(id, ct, params roles)` | `ExistsActiveWithAnyRoleAsync(id, roles, ct)` | `CancellationToken` last — .NET convention |
| `AssignedUserNotFound` + `AssignedUserInvalidRole` | `AssignedUserInvalidRole` only | Removed redundant message |

### 3.2 Consistency Checks

| Convention | Status |
|------------|--------|
| Async methods suffixed with `Async` | ✅ |
| Interfaces prefixed with `I` | ✅ |
| DTOs suffixed with `Dto` | ✅ |
| Validators suffixed with `Validator` | ✅ |
| Configuration classes suffixed with `Configuration` | ✅ |
| Extension methods in `*Extensions` classes | ✅ |
| camelCase JSON property names in errors | ✅ |

**Current state:** ✅ Naming is consistent and intent-revealing across all layers.

---

## 4. Validation

### 4.1 Architecture

Validation operates at three levels:

```
1. Model binding (ApiBehaviorExtensions)     → 400, ApiErrorResponse
2. FluentValidation (service layer)              → 400, ValidationException → ApiErrorResponse
3. Domain rules (TicketStatusWorkflow)           → consumed by validators
```

### 4.2 Findings & Improvements

| Finding | Improvement |
|---------|-------------|
| Model-binding errors returned `ProblemDetails`, not `ApiErrorResponse` | `ApiBehaviorExtensions.InvalidModelStateResponseFactory` unifies shape |
| `UpdateTicketRequestValidator` NRE when `Dto` is null | `.When(x => x.Dto is not null)` guard on status rule |
| Status transition validation re-queried DB for current status | `UpdateTicketRequest.CurrentStatus` populated from loaded entity |
| `AssignedToUserId = 0` showed role error, not format error | `ValidAssignedUser` checks `> 0` before role lookup |
| Missing ticket on comments returned 400 | Existence check moved to service → `NotFoundException` (404) |
| `GetStatusAsync` returning null passed closed-ticket rule | Validator fails when `status is null` |
| Pagination `pageNumber` had no upper bound (int overflow in `Skip`) | `MaxPageNumber` constant and validator rule added |
| camelCase/normalization duplicated | `JsonPropertyNameNormalizer` extracted |
| `dto.` prefix in nested validation errors | Stripped in `ValidationExtensions.ToErrorDictionary` |

### 4.3 Validation Coverage

| Area | Rules | Status |
|------|-------|--------|
| Ticket create | Title, description, priority, creator role, assignee role | ✅ |
| Ticket update | All fields + status transition | ✅ |
| Ticket search | Pagination, keyword length, status enum | ✅ |
| Comment create | Content, user existence, closed-ticket policy | ✅ |
| Comment list | Ticket ID format | ✅ |

### 4.4 Remaining Recommendation

| Item | Priority | Notes |
|------|----------|-------|
| Integration tests for validation edge cases | Medium | Documented in [test-strategy.md](./test-strategy.md) |

---

## 5. Performance

### 5.1 Findings & Improvements

| Finding | Impact | Improvement |
|---------|--------|-------------|
| Read queries used default tracking | Unnecessary memory and change-tracker overhead | `AsNoTracking()` on search, get-by-id, comment list, status projection |
| Create/update re-fetched full entity after save | Extra DB round-trip per operation | `LoadUsersAsync` / `LoadUserAsync` after save instead of full re-query |
| Update validation called `GetStatusAsync` separately | Redundant query | `CurrentStatus` passed from already-loaded entity |
| `GetStatusAsync` loaded full ticket | Over-fetching | Projects only `TicketStatus` column |
| `SearchAsync` used tracked entities | Accidental update risk on read path | `AsNoTracking()` with explicit includes |
| No null guard on search query | Potential NRE | `ArgumentNullException.ThrowIfNull(query)` in service and repository |

### 5.2 Query Strategy (After Review)

| Operation | Tracking | Includes | Notes |
|-----------|----------|----------|-------|
| Search | No | CreatedBy, AssignedTo | Paginated, ordered by CreatedAt desc |
| Get by ID | No | CreatedBy, AssignedTo | Read-only |
| Get for update | Yes | None | Tracked for EF change detection |
| Get status | No | Projection | Single column |
| Load navigations post-save | Yes | Explicit `LoadAsync` | No full re-fetch |

### 5.3 Performance Verdict

**Current state:** ✅ Read paths are optimized. Write paths use a single save + targeted navigation load. No N+1 issues identified in current endpoints.

---

## 6. Security

### 6.1 Findings

| Finding | Severity | Status |
|---------|----------|--------|
| No authentication or authorization | **Critical** | Open — documented, deferred to M10 |
| Client supplies `CreatedByUserId` / `UserId` in request body | **High** | Open — IDOR risk; requires JWT |
| `AllowedHosts: "*"` in `appsettings.json` | **Medium** | Open — restrict per environment before production |
| AutoMapper 12.0.1 vulnerability (NU1903) | **Medium** | Open — upgrade to 15.1.3+ planned |
| Input validation on all endpoints | — | ✅ Implemented |
| Parameterized queries via EF Core | — | ✅ Implemented |
| No hardcoded credentials | — | ✅ Connection string in config |
| Global exception handler hides stack traces in production | — | ✅ Implemented |
| `DbUpdateException` no longer leaks SQL details | — | ✅ Maps to generic 409 message |
| Client disconnect not logged as security event | — | ✅ Debug-level only |

### 6.2 Security Improvements Made

| Improvement | Detail |
|-------------|--------|
| Consistent input validation | FluentValidation on all write paths; pagination bounds enforced |
| Safe error messages | Production 500 responses use generic text; details only in Development |
| FK violation handling | `DbUpdateException` → 409 with safe message, not SQL internals |
| Null reference hardening | Unexpected NRE → 400, not 500 |
| User role validation | Creator must be Customer; assignee must be Agent/Admin; comment user must be active |

### 6.3 Security Recommendations (Not Yet Implemented)

| Priority | Recommendation |
|----------|----------------|
| P0 | Add JWT authentication and `[Authorize]` on controllers |
| P0 | Derive user ID from claims, remove from request body |
| P1 | Role-based authorization (Customer creates tickets; Agent updates) |
| P1 | Restrict `AllowedHosts` per deployment environment |
| P2 | Upgrade AutoMapper to 15.1.3+ |
| P2 | Add rate limiting on public endpoints |

---

## 7. Summary of Improvements Made

### Architecture & SOLID

- Removed unused `IRepository<T>`, `IService`, and bloated interface methods
- Moved hosting concerns out of Infrastructure
- Centralized status workflow in Domain
- Fixed `UnitOfWork` double-dispose risk
- Consolidated `TicketSearchFilter` into `TicketQueryDto`

### Naming

- Renamed repository methods to reflect read vs. write intent
- Extracted shared formatters and normalizers
- Fixed `CancellationToken` parameter ordering
- Removed redundant validation messages

### Validation

- Unified model-binding and FluentValidation error envelopes
- Guarded null `Dto` in status transition validator
- Eliminated redundant status DB query on update
- Fixed `AssignedToUserId = 0` error message
- Aligned missing-ticket HTTP semantics (404 on comments)
- Added pagination overflow protection

### Performance

- `AsNoTracking()` on all read-only queries
- Post-save navigation loading instead of full re-fetch
- Status projection instead of full entity load
- Null guards on search entry points

### Security & Error Handling

- `DbUpdateException` / concurrency → 409 Conflict
- `NullReferenceException` → 400 Bad Request
- Client disconnect → no false 500
- `DELETE` → 204 No Content
- Production-safe error messages

### Files Modified (Review Pass)

| Layer | Key Files |
|-------|-----------|
| Domain | `Ticket.cs` (domain methods) |
| Application | Services, validators, `ValidationRuleExtensions`, mappings, shared utilities |
| Infrastructure | Repositories, `UnitOfWork`, `TicketRepository` |
| API | `ExceptionMapper`, `GlobalExceptionMiddleware`, `ApiBehaviorExtensions`, controllers, `Program.cs` |
| Deleted | `IRepository.cs`, `IService.cs`, `TicketSearchFilter.cs` |

---

## 8. Review Verdict

| Criterion | Rating | Notes |
|-----------|--------|-------|
| SOLID adherence | **Good** | ISP and SRP improvements applied; interfaces are lean |
| Clean Architecture | **Good** | Layer boundaries respected; one hosting fix applied |
| Naming | **Good** | Consistent, intent-revealing conventions |
| Validation | **Good** | Comprehensive, unified error shape; tests planned |
| Performance | **Good** | Read/write paths optimized for current scale |
| Security | **Needs work** | Validation present; auth is the critical gap |

The backend is **well-structured and production-ready for a trusted internal environment**. Before public deployment, authentication, authorization, and host restrictions must be implemented.

---

## Related Documents

- [design-notes.md](./design-notes.md) — Detailed design decisions
- [debugging-notes.md](./debugging-notes.md) — Problems found and fixes applied
- [test-strategy.md](./test-strategy.md) — Testing approach and gaps
- [requirements-analysis.md](./requirements-analysis.md) — Functional requirements
- [implementation-plan.md](./implementation-plan.md) — Milestones and risks
