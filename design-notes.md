# Design Notes

**Project:** Support Ticket Management System  
**Purpose:** Document key architectural and design decisions for the backend API.

---

## 1. Clean Architecture

### 1.1 Principle

The solution separates concerns into four concentric layers. **Dependencies point inward** — outer layers depend on inner layers, never the reverse. The Domain layer has zero external dependencies.

```
         ┌──────────────────────────────────┐
         │           API                    │  HTTP, Swagger, middleware
         ├──────────────────────────────────┤
         │      Infrastructure              │  EF Core, SQL Server, repos
         ├──────────────────────────────────┤
         │      Application                 │  Use cases, DTOs, validation
         ├──────────────────────────────────┤
         │         Domain                   │  Entities, enums, rules
         └──────────────────────────────────┘
```

### 1.2 Layer Responsibilities

| Layer | Contains | Must Not Contain |
|-------|----------|------------------|
| **Domain** | `User`, `Ticket`, `Comment`, enums, `TicketStatusWorkflow`, `DomainException` | EF attributes, HTTP types, DTOs |
| **Application** | Services, DTOs, validators, repository interfaces, AutoMapper profiles, `ValidationException` | `DbContext`, controllers, SQL |
| **Infrastructure** | `ApplicationDbContext`, EF configurations, repository implementations, migrations, seed data | HTTP concerns, controller logic |
| **API** | Controllers, middleware, Swagger, `Program.cs`, `ApiErrorResponse` | Direct database access, business rules |

### 1.3 Dependency Registration

Each layer exposes a single DI entry point:

```csharp
// Program.cs
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
```

- **Application** registers services, validators, and AutoMapper
- **Infrastructure** registers `DbContext`, repositories, and `UnitOfWork`
- **API** composes both and configures middleware

### 1.4 Design Decisions

| Decision | Rationale |
|----------|-----------|
| Interfaces in Application, implementations in Infrastructure | Application defines contracts; Infrastructure is swappable (e.g., InMemory for tests) |
| No generic `IRepository<T>` exposed via DI | Interface Segregation — each repository exposes only what its consumers need |
| Domain workflow in `TicketStatusWorkflow` | Business rules live in Domain, not in validators or services alone |
| `ApiErrorResponse` in API layer | Error envelope is a presentation concern; `ValidationException` carries data from Application |

---

## 2. Repository Pattern

### 2.1 Overview

Data access is abstracted behind **entity-specific repository interfaces** defined in Application and implemented in Infrastructure. A shared abstract `Repository<T>` base class provides common CRUD helpers; specialized methods are added per entity.

```
Service  →  ITicketRepository (interface)  →  TicketRepository (implementation)  →  DbContext
```

### 2.2 Repository Interfaces

| Interface | Key Methods |
|-----------|-------------|
| `ITicketRepository` | `SearchAsync`, `GetByIdWithUsersAsync`, `GetByIdForUpdateAsync`, `AddAsync`, `DeleteAsync`, `ExistsAsync`, `GetStatusAsync`, `LoadUsersAsync` |
| `ICommentRepository` | `GetByTicketIdWithUserAsync`, `AddAsync`, `LoadUserAsync` |
| `IUserRepository` | `ExistsActiveAsync`, `ExistsActiveWithRoleAsync`, `ExistsActiveWithAnyRoleAsync` |

`IUserRepository` exposes only existence checks — no full CRUD — because the API does not manage user accounts directly.

### 2.3 Read vs. Write Queries

Repositories distinguish between read-optimized and write-tracked queries:

| Method | Tracking | Includes | Use Case |
|--------|----------|----------|----------|
| `GetByIdWithUsersAsync` | `AsNoTracking()` | `CreatedBy`, `AssignedTo` | GET by ID |
| `GetByIdForUpdateAsync` | Tracked | None | PUT, DELETE |
| `SearchAsync` | `AsNoTracking()` | `CreatedBy`, `AssignedTo` | List/search |
| `GetStatusAsync` | `AsNoTracking()` | Projection only | Validation |

This avoids unnecessary change tracking on read paths and prevents accidental updates on search results.

### 2.4 Post-Save Navigation Loading

After `SaveChangesAsync`, services call `LoadUsersAsync` / `LoadUserAsync` to populate navigation properties for AutoMapper — instead of re-fetching the entire entity:

```csharp
await _ticketRepository.AddAsync(ticket, cancellationToken);
await _unitOfWork.SaveChangesAsync(cancellationToken);
await _ticketRepository.LoadUsersAsync(ticket, cancellationToken);
return _mapper.Map<TicketDto>(ticket);
```

### 2.5 Unit of Work

`IUnitOfWork` exposes a single `SaveChangesAsync` method. Repositories stage changes; the service commits via the unit of work. `DbContext` lifetime is managed by DI — `UnitOfWork` does **not** dispose the context.

---

## 3. Service Layer

### 3.1 Role

Services are the **application use-case layer**. They sit between controllers and repositories, orchestrating:

1. Input validation (FluentValidation)
2. Entity mapping (AutoMapper)
3. Repository calls
4. Transaction commit (`UnitOfWork`)
5. Exception throwing (`NotFoundException`, etc.)

Controllers remain thin — they delegate to services and wrap results in `ApiResponse<T>`.

### 3.2 Service Inventory

| Service | Responsibilities |
|---------|-----------------|
| `TicketService` | CRUD, search/pagination, status transition validation |
| `CommentService` | List comments by ticket, add comment, ticket existence checks |

### 3.3 Typical Flow — Create Ticket

```
Controller.Create(dto)
  → TicketService.CreateAsync(dto)
      → _createValidator.ValidateDtoAsync(dto)         // FluentValidation
      → _mapper.Map<Ticket>(dto)                       // AutoMapper
      → _ticketRepository.AddAsync(ticket)             // Stage
      → _unitOfWork.SaveChangesAsync()                 // Commit
      → _ticketRepository.LoadUsersAsync(ticket)       // Load navigations
      → _mapper.Map<TicketDto>(ticket)                 // Return DTO
  → ApiResponse<TicketDto>.Ok(ticket)
```

### 3.4 Typical Flow — Update Ticket

```
TicketService.UpdateAsync(id, dto)
  → GetByIdForUpdateAsync(id)           // Tracked entity or NotFoundException
  → Build UpdateTicketRequest with CurrentStatus from entity
  → _updateRequestValidator.ValidateDtoAsync(request)   // Includes transition check
  → _mapper.Map(dto, ticket)            // Apply changes to tracked entity
  → _unitOfWork.SaveChangesAsync()
  → LoadUsersAsync → Map to DTO
```

Passing `CurrentStatus` from the loaded entity avoids a redundant database query for status during validation.

### 3.5 Comment Service — Validation Ordering

Comment operations enforce checks in a deliberate order:

1. **Format validation** — `ticketId > 0` (400)
2. **Existence check** — `NotFoundException` → 404
3. **Business validation** — DTO rules, closed-ticket policy (400)

This ensures missing tickets return **404**, not a misleading validation error.

---

## 4. Database Relationships

### 4.1 Entity Relationships

```
User 1 ──< creates ──< Ticket        (CreatedByUserId, required)
User 1 ──< assigned ──< Ticket       (AssignedToUserId, optional)
User 1 ──< authors ───< Comment      (UserId, required)
Ticket 1 ──< has ─────< Comment      (TicketId, required)
```

### 4.2 Foreign Key Delete Behaviors

| Relationship | FK Column | On Delete | Rationale |
|--------------|-----------|-----------|-----------|
| User → Ticket (creator) | `CreatedByUserId` | **Restrict** | Prevent deleting a user who created tickets |
| User → Ticket (assignee) | `AssignedToUserId` | **SetNull** | Allow user removal; ticket becomes unassigned |
| User → Comment | `UserId` | **Restrict** | Preserve comment authorship integrity |
| Ticket → Comment | `TicketId` | **Cascade** | Comments are owned by the ticket; deleted with it |

### 4.3 Indexes

| Table | Indexed Columns | Purpose |
|-------|----------------|---------|
| `Users` | `Email` (unique) | Prevent duplicate accounts |
| `Tickets` | `Status`, `Priority`, `CreatedByUserId`, `AssignedToUserId` | Filter and join performance |
| `Comments` | `TicketId`, `UserId` | Lookup by ticket and author |

### 4.4 Enum Storage

`TicketStatus`, `TicketPriority`, and `UserRole` are stored as **strings** in the database via `HasConversion<string>()`. This improves readability in raw SQL and avoids magic numbers, at the cost of slightly more storage.

### 4.5 Auditing

All entities inherit from `BaseEntity`:

```csharp
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

`ApplicationDbContext` sets `CreatedAt` on insert and `UpdatedAt` on modify. Seed data preserves explicit timestamps.

### 4.6 Seeding

Seed data runs at startup in Development and Testing via `ApplicationDbSeeder`. It is **idempotent** — records are inserted only when their ID is absent. SQL Server uses `IDENTITY_INSERT` to preserve seed IDs.

---

## 5. Validation Strategy

### 5.1 Two-Layer Approach

Validation operates at two levels, both returning the same `ApiErrorResponse` shape:

| Layer | When | Mechanism |
|-------|------|-----------|
| **Model binding** | Before the controller action runs | `ApiBehaviorExtensions.InvalidModelStateResponseFactory` |
| **FluentValidation** | Inside the service, before business logic | Validators + `ValidateDtoAsync` extension |

### 5.2 Validator Types

| Validator | Validates |
|-----------|-----------|
| `CreateTicketDtoValidator` | Title, description, priority, creator role (Customer), assignee role (Agent/Admin) |
| `UpdateTicketDtoValidator` | Title, description, status, priority, assignee |
| `UpdateTicketRequestValidator` | Ticket ID, nested DTO, **status transition** against `CurrentStatus` |
| `TicketQueryDtoValidator` | Page number/size bounds, keyword length, status enum |
| `CreateCommentDtoValidator` | Content length, active user existence |
| `CreateCommentRequestValidator` | Nested DTO, closed/cancelled ticket policy |
| `GetCommentsByTicketRequestValidator` | Ticket ID format (`> 0`) |

### 5.3 Shared Rule Extensions

Reusable validation rules live in `ValidationRuleExtensions`:

| Extension | Rule |
|-----------|------|
| `ValidTitle()` | Required, max 200 characters |
| `ValidDescription()` | Required, max 4000 characters |
| `ValidPriority()` | Must be valid `TicketPriority` enum |
| `ValidStatus()` | Must be valid `TicketStatus` enum |
| `ValidAssignedUser(repo)` | If provided: `> 0`, active Agent or Admin |
| `ValidTicketId()` | Must be `> 0` |

Constants (`ValidationConstants`) and messages (`ValidationMessages`) are centralized for consistency.

### 5.4 Cross-Field and Async Validation

- **Status transitions** — `UpdateTicketRequestValidator` compares `CurrentStatus` (from loaded entity) with `Dto.Status` via `TicketStatusWorkflow`
- **User existence** — `MustAsync` calls `IUserRepository` existence methods
- **Closed ticket comments** — `CreateCommentRequestValidator` calls `GetStatusAsync` and rejects `Closed` / `Cancelled`

### 5.5 Cascade Modes

```csharp
ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;    // Stop at first failure per property
ValidatorOptions.Global.DefaultClassLevelCascadeMode = CascadeMode.Continue; // Run all properties
```

Status transition rules use `.When(x => x.Dto is not null)` to prevent null-reference errors when the request body is missing.

### 5.6 Error Shape Normalization

`ValidationExtensions.ToErrorDictionary` ensures:

- Property names are **camelCase** (via `JsonPropertyNameNormalizer`)
- Nested `dto.` prefixes are stripped (e.g., `dto.title` → `title`)
- Duplicate messages per field are deduplicated

---

## 6. Exception Handling

### 6.1 Pipeline

```
Request
  → Controller / Service throws exception
  → GlobalExceptionMiddleware catches
  → ExceptionMapper.Map(exception) → status code, message, errors
  → ApiErrorResponse serialized as JSON
```

Middleware is registered **first** in the pipeline so it wraps all downstream components.

### 6.2 Exception Taxonomy

| Exception | HTTP Status | Source | Field Errors |
|-----------|-------------|--------|--------------|
| `ValidationException` | 400 | FluentValidation failures | Yes |
| `DomainException` | 400 | Domain rules (e.g., invalid transition) | No |
| `NotFoundException` | 404 | Missing ticket/entity | No |
| `ArgumentNullException` | 400 | Null arguments | No |
| `NullReferenceException` | 400 | Unexpected null dereference | No |
| `ArgumentException` | 400 | Invalid arguments | No |
| `InvalidOperationException` | 400 | Invalid state | No |
| `UnauthorizedAccessException` | 401 | Future auth | No |
| `KeyNotFoundException` | 404 | Missing key | No |
| `DbUpdateConcurrencyException` | 409 | Concurrent modification | No |
| `DbUpdateException` | 409 | FK violation, constraint failure | No |
| Unhandled | 500 | Unexpected errors | No (details in Development only) |

### 6.3 Error Response Contract

```json
{
  "success": false,
  "statusCode": 400,
  "message": "One or more validation errors occurred.",
  "errors": {
    "title": ["Title is required."]
  },
  "traceId": "00-abc123..."
}
```

In **Development**, unhandled exceptions include a `details` field with the full stack trace. In **Production**, a generic message is returned.

### 6.4 Logging

| Severity | When |
|----------|------|
| `LogLevel.Warning` | All handled exceptions (4xx) |
| `LogLevel.Error` | Unhandled exceptions (5xx) |

Each log entry includes exception type, HTTP status, request method, path, and trace ID.

### 6.5 Special Cases

| Scenario | Behavior |
|----------|----------|
| Client disconnect (`OperationCanceledException`) | Logged at Debug; no error response written |
| Response already started | Warning logged; exception rethrown (cannot rewrite body) |
| Model-binding failure | Handled by `ApiBehaviorExtensions` before middleware — same `ApiErrorResponse` shape |

### 6.6 Application vs. Domain Exceptions

| Type | Layer | Purpose |
|------|-------|---------|
| `ValidationException` | Application | Input validation failures with field-level errors |
| `NotFoundException` | Application | Resource not found by ID |
| `DomainException` | Domain | Business rule violations without field mapping |

Services throw `NotFoundException` when a repository returns null. Validators throw `ValidationException` via `ValidateDtoAsync`. Domain entities can throw `DomainException` for workflow violations (available for future use; transitions are currently enforced in validators).

---

## Related Documents

- [implementation-plan.md](./implementation-plan.md) — Milestones and delivery plan
- [requirements-analysis.md](./requirements-analysis.md) — Functional and non-functional requirements
- [README.md](./README.md) — Setup and API reference
