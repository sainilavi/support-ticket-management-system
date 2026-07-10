# Debugging Notes

**Project:** Support Ticket Management System  
**Purpose:** Record of problems encountered during development, their root causes, fixes applied, and how each fix was validated.

> All fixes were manually reviewed before acceptance. Each entry was verified with `dotnet build` and, where applicable, `dotnet test`.

---

## Summary

| Category | Issues | Resolved |
|----------|--------|----------|
| Tooling & environment | 4 | 4 |
| Build & compilation | 6 | 6 |
| EF Core & database | 3 | 3 |
| Validation & errors | 5 | 5 |
| HTTP & API behavior | 4 | 4 |
| Architecture & refactoring | 4 | 4 |
| Testing | 2 | 2 |
| **Total** | **28** | **28** |

---

## 1. Tooling & Environment

### 1.1 PowerShell command separator

| | |
|---|---|
| **Problem** | Shell commands using `&&` failed with `InvalidEndOfLine` on Windows PowerShell |
| **Root cause** | `&&` is not a valid statement separator in older PowerShell versions |
| **Fix applied** | Replaced `&&` with `;` in all terminal commands |
| **Validation** | Subsequent `dotnet build`, `dotnet ef`, and `dotnet test` commands executed successfully |

### 1.2 EF Core CLI not found

| | |
|---|---|
| **Problem** | `dotnet ef migrations add` failed — command not recognized |
| **Root cause** | `dotnet-ef` global tool was not installed; `Microsoft.EntityFrameworkCore.Design` was missing from the startup project |
| **Fix applied** | Added `dotnet-tools.json` (pins `dotnet-ef` 8.0.11); added `Microsoft.EntityFrameworkCore.Design` to the API project |
| **Validation** | `dotnet tool restore` + `dotnet ef migrations add InitialCreate` completed successfully |

### 1.3 Connection string hardcoded in Program.cs

| | |
|---|---|
| **Problem** | User reported connection string should not live in `Program.cs`; EF tooling packages were missing |
| **Root cause** | Initial scaffold placed configuration inline; Design package required for migrations was absent from API project |
| **Fix applied** | Connection string moved to `appsettings.json`; `AddInfrastructure(builder.Configuration)` reads `DefaultConnection`; EF Design package added to API |
| **Validation** | App starts with config-based connection; `dotnet ef database update` works |

### 1.4 AutoMapper package version mismatch

| | |
|---|---|
| **Problem** | Build warning/error from incompatible AutoMapper and DI extension package versions |
| **Root cause** | `AutoMapper` and `AutoMapper.Extensions.Microsoft.DependencyInjection` were on different major versions |
| **Fix applied** | Aligned both packages to **12.0.1** |
| **Validation** | `dotnet build` succeeded with no package resolution errors |

---

## 2. Build & Compilation

### 2.1 ValidationException name ambiguity

| | |
|---|---|
| **Problem** | `ValidationException` was ambiguous between `FluentValidation.ValidationException` and `Application.Exceptions.ValidationException` |
| **Root cause** | Both namespaces imported in `ValidationExtensions.cs` and `TicketService.cs` |
| **Fix applied** | Added type alias `using AppValidationException = ...Application.Exceptions.ValidationException` in `ValidationExtensions`; used fully qualified name in `TicketService` |
| **Validation** | `dotnet build` — zero errors |

### 2.2 Missing using in ITicketRepository

| | |
|---|---|
| **Problem** | `TicketStatus` type not found in `ITicketRepository.cs` after adding `GetStatusAsync` |
| **Root cause** | Missing `using SupportTicketManagementSystem.Domain.Enums` |
| **Fix applied** | Added the required using directive |
| **Validation** | `dotnet build` succeeded |

### 2.3 Validation namespace error in ValidationException.cs

| | |
|---|---|
| **Problem** | `error CS0103: The name 'Validation' does not exist` during architecture refactor |
| **Root cause** | Incorrect reference `Validation.ValidationMessages` instead of proper namespace import |
| **Fix applied** | Added `using SupportTicketManagementSystem.Application.Common.Validation` and referenced `ValidationMessages` directly |
| **Validation** | `dotnet build` succeeded |

### 2.4 IRepository removal broke Repository base class

| | |
|---|---|
| **Problem** | Build failed after deleting `IRepository<T>` — `Repository<T>` still implemented the removed interface |
| **Root cause** | Architecture refactor removed generic repository interface but not the inheritance |
| **Fix applied** | Changed `Repository<T>` to a standalone `abstract class` with no interface; removed `UpdateAsync` (EF change tracking handles updates) |
| **Validation** | `dotnet build`; all 14 integration tests passed |

### 2.5 IHost not available in Infrastructure layer

| | |
|---|---|
| **Problem** | `error CS0234: Hosting does not exist` in `HostDatabaseExtensions.cs` |
| **Root cause** | `Microsoft.Extensions.Hosting` is not a dependency of the Infrastructure project; hosting concerns violate Clean Architecture boundaries |
| **Fix applied** | Moved `HostDatabaseExtensions` from Infrastructure to `API/Extensions/` |
| **Validation** | `dotnet build` succeeded; database initialization works in Development and Testing |

### 2.6 AutoMapper ForAllMaps unavailable

| | |
|---|---|
| **Problem** | `error CS1061: ForAllMaps not found` when adding security MaxDepth configuration |
| **Root cause** | `ForAllMaps` was introduced in a later AutoMapper version; project uses 12.0.1 |
| **Fix applied** | Reverted the `ForAllMaps` configuration; documented AutoMapper upgrade as a future improvement |
| **Validation** | `dotnet build` succeeded |

---

## 3. EF Core & Database

### 3.1 Enum default value migration warnings

| | |
|---|---|
| **Problem** | EF Core warned about `HasDefaultValue` on enum properties stored as strings |
| **Root cause** | `HasDefaultValue(TicketStatus.Open)` and `HasDefaultValue(TicketPriority.Medium)` are incompatible with string conversion |
| **Fix applied** | Removed `HasDefaultValue` from `TicketConfiguration`; defaults set in domain entity and AutoMapper profile instead |
| **Validation** | Migration regenerated cleanly; `dotnet ef migrations add` produced no warnings |

### 3.2 Duplicate seed data on restart

| | |
|---|---|
| **Problem** | Seed records duplicated when the application restarted |
| **Root cause** | `HasData` in EF configuration combined with runtime seeding caused double inserts |
| **Fix applied** | Removed `HasData` from `UserConfiguration`; moved all seeding to idempotent `ApplicationDbSeeder` (checks existing IDs before insert) |
| **Validation** | Multiple app restarts in Development — no duplicate users, tickets, or comments |

### 3.3 SQL Server IDENTITY_INSERT for seed IDs

| | |
|---|---|
| **Problem** | Seed data with explicit IDs (1, 2, 3) failed on SQL Server without identity override |
| **Root cause** | SQL Server identity columns reject explicit ID inserts by default |
| **Fix applied** | `ApplicationDbSeeder` wraps seed inserts in `SET IDENTITY_INSERT ON/OFF` for SQL Server; InMemory provider uses direct insert |
| **Validation** | Seed users available at expected IDs; integration tests use IDs 2 (Agent) and 3 (Customer) successfully |

---

## 4. Validation & Error Handling

### 4.1 Inconsistent validation error response shape

| | |
|---|---|
| **Problem** | FluentValidation errors returned `ApiErrorResponse`, but model-binding failures returned ASP.NET `ProblemDetails` |
| **Root cause** | `[ApiController]` auto-validation uses a different response factory than the global exception middleware |
| **Fix applied** | Added `ApiBehaviorExtensions` with custom `InvalidModelStateResponseFactory` returning `ApiErrorResponse` with camelCase field keys |
| **Validation** | `dotnet build`; manual verification of malformed JSON returns consistent error envelope |

### 4.2 NullReferenceException on null UpdateTicketDto

| | |
|---|---|
| **Problem** | Null request body on `PUT /api/tickets/{id}` could cause 500 instead of 400 |
| **Root cause** | `UpdateTicketRequestValidator` status rule dereferenced `request.Dto.Status` even when `Dto` was null (`CascadeMode.Continue` on class level) |
| **Fix applied** | Added `.When(x => x.Dto is not null)` guard on the status transition rule |
| **Validation** | `dotnet build`; 14 integration tests passed |

### 4.3 AssignedToUserId = 0 misleading error

| | |
|---|---|
| **Problem** | `AssignedToUserId: 0` returned "must be an active agent or admin" instead of "must be greater than 0" |
| **Root cause** | `ValidAssignedUser` only skipped validation for `null`, not for zero |
| **Fix applied** | Added `Must(id => !id.HasValue \|\| id.Value > 0)` before the role existence check |
| **Validation** | `dotnet build` |

### 4.4 Null ticket status treated as comment-allowed

| | |
|---|---|
| **Problem** | `GetStatusAsync` returning `null` for a missing ticket passed the closed-ticket comment validator |
| **Root cause** | `null is not (Closed or Cancelled)` evaluates to `true` in C# pattern matching |
| **Fix applied** | Validator now returns `false` when `status is null`; service layer checks ticket existence before business validation |
| **Validation** | `dotnet build`; 14 integration tests passed |

### 4.5 DbUpdateException returned 500

| | |
|---|---|
| **Problem** | FK violations (e.g., user deleted between validation and save) returned 500 Internal Server Error |
| **Root cause** | `ExceptionMapper` had no handler for `DbUpdateException` or `DbUpdateConcurrencyException` |
| **Fix applied** | Both mapped to **409 Conflict** with a generic, safe message |
| **Validation** | `dotnet build` |

---

## 5. HTTP & API Behavior

### 5.1 Missing ticket returned 400 on comment endpoints

| | |
|---|---|
| **Problem** | `GET/POST /api/tickets/{ticketId}/comments` returned 400 for non-existent tickets; ticket endpoints returned 404 |
| **Root cause** | `ValidTicketId` used FluentValidation with "Ticket does not exist" message (400); ticket services threw `NotFoundException` (404) |
| **Fix applied** | Removed existence check from `ValidTicketId` (format only); `CommentService.EnsureTicketExistsAsync` throws `NotFoundException` |
| **Validation** | `dotnet build`; 14 integration tests passed |

### 5.2 DELETE returned 200 with empty body

| | |
|---|---|
| **Problem** | `DELETE /api/tickets/{id}` returned 200 OK with an empty `ApiResponse<object>` |
| **Root cause** | Controller returned `Ok(ApiResponse<object>.Ok(new object(), ...))` |
| **Fix applied** | Changed to `return NoContent()` (204) |
| **Validation** | `dotnet build`; Swagger attributes updated to document 204 |

### 5.3 NullReferenceException unmapped to 500

| | |
|---|---|
| **Problem** | Unexpected null dereferences returned 500 instead of a client-friendly error |
| **Root cause** | `ExceptionMapper` handled `ArgumentNullException` but not `NullReferenceException` |
| **Fix applied** | Added `NullReferenceException` → 400 with message "A required value was missing." |
| **Validation** | `dotnet build` |

### 5.4 Client disconnect logged as server error

| | |
|---|---|
| **Problem** | `OperationCanceledException` on aborted requests was logged at Error level and returned 500 |
| **Root cause** | Global exception middleware treated all exceptions uniformly |
| **Fix applied** | Added filter: when `RequestAborted.IsCancellationRequested`, log at Debug and skip error response |
| **Validation** | `dotnet build` |

---

## 6. Architecture & Refactoring

### 6.1 Redundant database round-trips on create/update

| | |
|---|---|
| **Problem** | After creating or updating a ticket, the service re-fetched the full entity from the database |
| **Root cause** | `GetByIdWithUsersAsync` called again post-save instead of loading navigations on the tracked entity |
| **Fix applied** | Added `LoadUsersAsync` / `LoadUserAsync` to repositories; services call these after `SaveChangesAsync` |
| **Validation** | `dotnet build`; integration tests confirm creator/assignee names in responses |

### 6.2 Duplicate user name formatting

| | |
|---|---|
| **Problem** | `FormatUserName` logic duplicated in `TicketProfile` and `CommentProfile` |
| **Root cause** | Each AutoMapper profile implemented its own formatting helper |
| **Fix applied** | Extracted `UserDisplayNameFormatter.Format(User?)` with null guard |
| **Validation** | `dotnet build`; ticket and comment responses include correct display names |

### 6.3 Duplicate camelCase normalization

| | |
|---|---|
| **Problem** | Property name camelCasing logic duplicated in `ValidationExtensions` and `ValidationException` |
| **Root cause** | Each class implemented its own string transformation |
| **Fix applied** | Extracted `JsonPropertyNameNormalizer.ToCamelCase` shared utility |
| **Validation** | `dotnet build`; invalid status transition tests confirm `errors.status` key is present |

### 6.4 UnitOfWork disposing DbContext

| | |
|---|---|
| **Problem** | `UnitOfWork.Dispose()` called `_context.Dispose()`, risking double-dispose with DI lifetime |
| **Root cause** | `IUnitOfWork` inherited `IDisposable` and manually disposed the scoped `DbContext` |
| **Fix applied** | Removed `IDisposable` from `IUnitOfWork`; DI container owns `DbContext` lifetime |
| **Validation** | `dotnet build`; integration tests pass without disposal errors |

---

## 7. Testing

### 7.1 WebApplicationFactory could not reference Program

| | |
|---|---|
| **Problem** | Integration tests could not use `WebApplicationFactory<Program>` |
| **Root cause** | Top-level statements in `Program.cs` generate an internal `Program` class by default |
| **Fix applied** | Added `public partial class Program;` at the end of `Program.cs` |
| **Validation** | Test project builds; `WebApplicationFactory<Program>` initializes correctly |

### 7.2 Duplicate database initialization in test factory

| | |
|---|---|
| **Problem** | `CustomWebApplicationFactory.CreateHost` and `Program.InitializeDatabaseAsync` both seeded data |
| **Root cause** | Factory overrode `CreateHost` with manual `EnsureCreated` + seed after `Program.cs` was updated to initialize on startup |
| **Fix applied** | Removed manual initialization from factory; `HostDatabaseExtensions` handles `EnsureCreated` + seed for `Testing` environment |
| **Validation** | 14 integration tests pass; no duplicate seed errors |

---

## 8. Swagger & Documentation

### 8.1 Null controller in DocInclusionPredicate

| | |
|---|---|
| **Problem** | Swagger generation could fail when `controller` route value was missing |
| **Root cause** | `DocInclusionPredicate` accessed `controller` without null check |
| **Fix applied** | Added null/whitespace guard returning `false` for unmatched descriptors |
| **Validation** | Swagger UI loads with grouped Tickets and Comments docs |

### 8.2 Application XML comments not visible in Swagger

| | |
|---|---|
| **Problem** | DTO XML comments from Application layer were missing in Swagger |
| **Root cause** | Swashbuckle only reads XML docs from the API project's output directory |
| **Fix applied** | Added MSBuild `CopyApplicationXmlDocumentation` target in API `.csproj` |
| **Validation** | Swagger displays XML comments on DTO properties in request/response schemas |

### 8.3 Bare `throw` in exception middleware

| | |
|---|---|
| **Problem** | `throw;` inside middleware lost the original exception context in some paths |
| **Root cause** | Used bare rethrow when response had already started |
| **Fix applied** | Changed to `throw exception;` to preserve the caught exception instance |
| **Validation** | `dotnet build` |

---

## 9. Validation Checklist

Every fix in this document was validated using one or more of the following:

| Check | Command / Action |
|-------|-----------------|
| Compilation | `dotnet build` — zero errors |
| Integration tests | `dotnet test tests/SupportTicketManagementSystem.IntegrationTests` — 14/14 pass |
| EF migrations | `dotnet ef migrations add` / `dotnet ef database update` |
| Manual API check | Swagger UI at `/swagger` in Development |
| Code review | Manual review of AI-generated changes before acceptance |

---

## 10. Open Items (Not Yet Debugged / Fixed)

| Item | Status | Notes |
|------|--------|-------|
| AutoMapper 12.0.1 security advisory (NU1903) | Open | Requires upgrade to 15.1.3+; `ForAllMaps` unavailable in 12.x |
| No authentication — public API | Open | Documented; not a bug but a known security gap |
| Comment API integration tests | Open | Covered in [test-strategy.md](./test-strategy.md) roadmap |
| 409 Conflict path untested | Open | InMemory provider does not reproduce FK races reliably |

---

## Related Documents

- [tool-workflow.md](./tool-workflow.md) — Cursor AI development workflow
- [design-notes.md](./design-notes.md) — Architecture and exception handling design
- [test-strategy.md](./test-strategy.md) — Testing approach and coverage roadmap
- [implementation-plan.md](./implementation-plan.md) — Risks and mitigations
