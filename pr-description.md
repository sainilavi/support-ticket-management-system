# Pull Request: Support Ticket Management System — Backend API

## Summary

This PR introduces the complete backend for the **Support Ticket Management System** — an ASP.NET Core 8 REST API built with Clean Architecture. It delivers ticket lifecycle management, threaded comments, search with pagination, FluentValidation, global exception handling, Swagger documentation, EF Core migrations, and integration tests.

The solution is organized into four layers (Domain, Application, Infrastructure, API) with strict inward dependency flow. Development was AI-assisted via Cursor with all generated code manually reviewed and verified through build and test gates.

---

## Features Implemented

### Ticket Management
- Create, read, update, and delete support tickets
- Default status `Open` on creation
- Search by keyword (title/description) with status filter and pagination
- Paginated results with `totalCount`, `totalPages`, and `hasNextPage` metadata

### Status Workflow
Enforced transitions with validation on update:

| From | Allowed |
|------|---------|
| `Open` | `InProgress`, `Cancelled` |
| `InProgress` | `Resolved`, `Cancelled` |
| `Resolved` | `Closed` |
| `Closed` / `Cancelled` | Terminal (no transitions) |

### Comments
- Add comments to active tickets
- List comments by ticket (ordered by `CreatedAt` ascending)
- Block comments on `Closed` and `Cancelled` tickets

### Cross-Cutting
- **FluentValidation** — field-level and cross-field rules (roles, lengths, enums, transitions)
- **Global exception middleware** — maps exceptions to `ApiErrorResponse` with correct HTTP status codes
- **Unified error envelope** — model-binding and FluentValidation errors use the same `ApiErrorResponse` shape
- **Swagger** — grouped docs (Tickets / Comments), XML comments, sample requests/responses
- **AutoMapper** — entity ↔ DTO mapping with shared `UserDisplayNameFormatter`
- **Seed data** — idempotent runtime seeder for users, tickets, and comments (Development / Testing)

### API Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/tickets` | Search with `keyword`, `status`, `pageNumber`, `pageSize` |
| `GET` | `/api/tickets/{id}` | Get ticket by ID |
| `POST` | `/api/tickets` | Create ticket |
| `PUT` | `/api/tickets/{id}` | Update ticket |
| `DELETE` | `/api/tickets/{id}` | Delete ticket (204 No Content) |
| `GET` | `/api/tickets/{ticketId}/comments` | List comments |
| `POST` | `/api/tickets/{ticketId}/comments` | Add comment |

---

## Database Changes

### Migrations

| Migration | Description |
|-----------|-------------|
| `20260706051544_InitialCreate` | Creates `Users`, `Tickets`, `Comments` tables with indexes and foreign keys |
| `20260710064156_MoveSeedToRuntimeSeeder` | Schema snapshot update; seed data moved from EF `HasData` to runtime seeder |

### Schema

**Users** — `Id`, `FirstName`, `LastName`, `Email` (unique), `PasswordHash`, `Role`, `IsActive`, `CreatedAt`, `UpdatedAt`

**Tickets** — `Id`, `Title`, `Description`, `Status`, `Priority`, `CreatedByUserId`, `AssignedToUserId`, `CreatedAt`, `UpdatedAt`

**Comments** — `Id`, `Content`, `TicketId`, `UserId`, `CreatedAt`, `UpdatedAt`

### Relationships & Delete Behaviors

| Relationship | On Delete |
|--------------|-----------|
| User → Ticket (creator) | Restrict |
| User → Ticket (assignee) | SetNull |
| User → Comment | Restrict |
| Ticket → Comment | Cascade |

### Seed Data (Development / Testing)

| Entity | Records | IDs |
|--------|---------|-----|
| Users | 3 | Admin (1), Agent (2), Customer (3) |
| Tickets | 3 | Sample tickets in various statuses |
| Comments | 3 | Sample thread on seed tickets |

Seeding is **idempotent** — no duplicates on repeated application starts.

### Apply Migrations

```bash
dotnet ef database update \
  --project src/SupportTicketManagementSystem.Infrastructure \
  --startup-project src/SupportTicketManagementSystem.API
```

---

## Testing

### Integration Tests

**Project:** `tests/SupportTicketManagementSystem.IntegrationTests`  
**Framework:** xUnit + `WebApplicationFactory<Program>` + EF Core InMemory  
**Result:** **14 / 14 passing**

| Test Area | Coverage |
|-----------|----------|
| Create ticket | ✅ Valid payload → 201, correct fields and default status |
| Update ticket | ✅ Valid payload → 200, fields updated |
| Valid status transitions | ✅ 5 transition pairs (theory-driven) |
| Invalid status transitions | ✅ 7 rejected pairs → 400 with `status` field error |
| Search | ✅ Keyword filter, status filter, pagination metadata |

### Run Tests

```bash
dotnet test tests/SupportTicketManagementSystem.IntegrationTests
```

### Not Yet Covered (follow-up PRs)

- Comment API endpoints
- GET / DELETE ticket (404 cases)
- Validation edge cases (required fields, max lengths, user roles)
- Model-binding error shape
- Auth scenarios

---

## AI Usage Summary

This project was built iteratively using **Cursor AI (Agent mode)** with human review at every step.

| Phase | AI Role |
|-------|---------|
| Scaffolding | Generated Clean Architecture solution structure, DI, middleware skeleton |
| Features | Implemented vertical slices (DTOs → validators → services → repos → controllers) per prompt |
| Review | Audited SOLID, architecture, performance, security; applied 28 fixes |
| Bug fixes | Resolved null references, HTTP status inconsistencies, validation gaps |
| Documentation | Generated README, design notes, test strategy, and supporting docs |

### Workflow

- **One feature per prompt** — scoped, reviewable increments
- **Build gate** — `dotnet build` after every change
- **Test gate** — `dotnet test` after feature and fix passes
- **Manual review** — all AI-generated code reviewed before acceptance

> Full details: [tool-workflow.md](./tool-workflow.md) · [reflection.md](./reflection.md)

---

## Known Limitations

| Limitation | Impact | Planned |
|------------|--------|---------|
| **No authentication / authorization** | API is fully public; any caller can impersonate users via request body IDs | JWT + role-based access (M10) |
| **User IDs in request body** | `CreatedByUserId` and `UserId` supplied by client — IDOR risk | Derive from claims after auth |
| **Comment APIs untested** | No integration tests for comment endpoints | v1.1 test pass |
| **No comment pagination** | All comments loaded per ticket | v1.1 |
| **AutoMapper 12.0.1 advisory (NU1903)** | Known vulnerability in dependency | Upgrade to 15.1.3+ |
| **`AllowedHosts: *`** | Permissive host header in config | Restrict per environment before production |
| **Hard delete** | Deleted tickets are permanently removed | Soft delete considered for v2 |
| **InMemory tests only** | FK behaviors may differ from SQL Server | SQL Server smoke tests in CI (future) |

---

## Project Structure

```
src/
├── SupportTicketManagementSystem.Domain/
├── SupportTicketManagementSystem.Application/
├── SupportTicketManagementSystem.Infrastructure/
└── SupportTicketManagementSystem.API/
tests/
└── SupportTicketManagementSystem.IntegrationTests/
```

---

## Checklist

- [x] Solution builds with zero errors (`dotnet build`)
- [x] Integration tests pass (`dotnet test` — 14/14)
- [x] EF Core migrations included
- [x] Swagger documentation configured
- [x] FluentValidation on all write paths
- [x] Global exception handling with consistent error envelope
- [x] Seed data is idempotent
- [x] README and supporting documentation included
- [ ] Authentication implemented (deferred)
- [ ] Comment API integration tests (deferred)

---

## Related Documentation

| Document | Description |
|----------|-------------|
| [README.md](./README.md) | Setup, API reference, run instructions |
| [requirements-analysis.md](./requirements-analysis.md) | Business and functional requirements |
| [implementation-plan.md](./implementation-plan.md) | Milestones and delivery plan |
| [design-notes.md](./design-notes.md) | Architecture and design decisions |
| [code-review-notes.md](./code-review-notes.md) | SOLID and security review |
| [test-strategy.md](./test-strategy.md) | Testing approach and roadmap |
| [debugging-notes.md](./debugging-notes.md) | Issues encountered and fixes |
| [reflection.md](./reflection.md) | AI-assisted development reflection |
