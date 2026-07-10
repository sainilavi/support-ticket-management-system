# Test Strategy

**Project:** Support Ticket Management System  
**Test Framework:** xUnit · ASP.NET Core Test Host · EF Core InMemory  
**Current Suite:** 14 integration tests (all passing)

---

## 1. Testing Philosophy

The project prioritizes **integration tests over unit tests** for v1. Integration tests exercise the full HTTP pipeline — routing, model binding, validation, services, repositories, middleware, and serialization — against a real application host with an isolated in-memory database.

| Principle | Rationale |
|-----------|-----------|
| Test through HTTP | Validates the contract clients actually consume |
| Isolate per test run | Unique in-memory database per factory instance |
| Assert status codes and response shape | Catches regressions in error handling and envelopes |
| Use theory tests for workflows | Covers combinatorial status transitions efficiently |
| Defer unit tests to v2 | Validators and domain rules are cheap to add later |

---

## 2. Integration Testing

### 2.1 Architecture

```
TicketsApiTests
    └── IClassFixture<CustomWebApplicationFactory>
            └── WebApplicationFactory<Program>
                    ├── Testing environment
                    ├── In-memory EF Core database (unique per factory)
                    ├── Full DI container (Application + Infrastructure)
                    ├── Global exception middleware
                    └── Seed data via InitializeDatabaseAsync
```

### 2.2 Test Host Setup

`CustomWebApplicationFactory` replaces SQL Server with an **EF Core InMemory** provider:

```csharp
builder.UseEnvironment("Testing");

services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase($"IntegrationTests_{Guid.NewGuid()}"));
```

| Setting | Value | Purpose |
|---------|-------|---------|
| Environment | `Testing` | Skips HTTPS redirect; triggers `EnsureCreated` + seed |
| Database | InMemory (unique name) | No SQL Server dependency; isolated state |
| Startup | Full `Program.cs` pipeline | Real middleware, DI, and configuration |

### 2.3 Test Project Structure

```
tests/SupportTicketManagementSystem.IntegrationTests/
├── CustomWebApplicationFactory.cs   # Host and database override
├── HttpClientJsonExtensions.cs      # JSON POST/PUT/GET helpers
├── TicketsApiTests.cs               # Ticket API test cases
└── SupportTicketManagementSystem.IntegrationTests.csproj
```

### 2.4 Dependencies

| Package | Version | Role |
|---------|---------|------|
| `Microsoft.AspNetCore.Mvc.Testing` | 8.0.11 | `WebApplicationFactory` |
| `Microsoft.EntityFrameworkCore.InMemory` | 8.0.11 | In-memory database |
| `xunit` | 2.5.3 | Test runner and assertions |
| `Microsoft.NET.Test.Sdk` | 17.8.0 | Test discovery and execution |
| `coverlet.collector` | 6.0.0 | Code coverage collection |

### 2.5 Seed Data in Tests

Tests rely on seeded users (IDs 1–3) from `ApplicationDbSeeder`:

| ID | Role | Used In Tests |
|----|------|---------------|
| 1 | Admin | Available for assignee scenarios |
| 2 | Agent | Default `AssignedToUserId` |
| 3 | Customer | Default `CreatedByUserId` |

### 2.6 Running Tests

```bash
# Run all integration tests
dotnet test tests/SupportTicketManagementSystem.IntegrationTests

# Run with verbose output
dotnet test tests/SupportTicketManagementSystem.IntegrationTests --logger "console;verbosity=detailed"

# Build before test (CI pipeline)
dotnet build && dotnet test --no-build
```

---

## 3. API Testing

### 3.1 Approach

API tests send real HTTP requests via `HttpClient` and assert on **status codes**, **response envelopes**, and **payload content**. JSON serialization uses `JsonStringEnumConverter` to match API behavior.

### 3.2 HTTP Helpers

`HttpClientJsonExtensions` provides:

| Method | Purpose |
|--------|---------|
| `PostJsonAsync<T>(uri, payload)` | POST with JSON body |
| `PutJsonAsync<T>(uri, payload)` | PUT with JSON body |
| `ReadAsJsonAsync<T>(response)` | Deserialize response with case-insensitive property matching |

### 3.3 Response Assertions

Every test verifies the appropriate envelope:

| Outcome | Type | Key Assertions |
|---------|------|----------------|
| Success | `ApiResponse<T>` | `success == true`, `data` not null, field values match |
| Validation failure | `ApiErrorResponse` | `statusCode == 400`, `errors` dictionary present |
| Not found | `ApiErrorResponse` | `statusCode == 404` (planned) |

### 3.4 Current API Test Coverage

| Endpoint | Method | Test | Status |
|----------|--------|------|--------|
| `/api/tickets` | `POST` | `CreateTicket_ReturnsCreatedTicket` | ✅ |
| `/api/tickets/{id}` | `PUT` | `UpdateTicket_ReturnsUpdatedTicket` | ✅ |
| `/api/tickets` | `GET` | `SearchTickets_FiltersByKeywordStatusAndPagination` | ✅ |
| `/api/tickets/{id}` | `GET` | — | Planned |
| `/api/tickets/{id}` | `DELETE` | — | Planned |
| `/api/tickets/{ticketId}/comments` | `GET` | — | Planned |
| `/api/tickets/{ticketId}/comments` | `POST` | — | Planned |

### 3.5 Planned API Tests

| Test | Expected Result |
|------|-----------------|
| `GetTicket_ExistingId_Returns200` | Ticket with creator/assignee names |
| `GetTicket_NonExistentId_Returns404` | `ApiErrorResponse` with 404 |
| `DeleteTicket_ExistingId_Returns204` | Empty body, 204 status |
| `DeleteTicket_NonExistentId_Returns404` | `ApiErrorResponse` with 404 |
| `CreateComment_ValidRequest_Returns201` | Comment with user name |
| `GetComments_ExistingTicket_ReturnsOrderedList` | Oldest first |
| `CreateComment_ClosedTicket_Returns400` | Error on `ticketId` |
| `GetComments_NonExistentTicket_Returns404` | `ApiErrorResponse` with 404 |

---

## 4. Status Workflow Testing

### 4.1 Workflow Under Test

```
Open ──→ InProgress ──→ Resolved ──→ Closed
  │           │
  └──→ Cancelled ←──┘
```

Terminal states (`Closed`, `Cancelled`) accept no further transitions.

### 4.2 Test Strategy

Status workflow is tested with **xUnit `[Theory]` + `[InlineData]`** to cover multiple transition pairs in parameterized tests.

#### Valid transitions — `UpdateTicket_AllowsValidStatusTransitions`

| From | To | Expected |
|------|----|----------|
| `Open` | `InProgress` | 200 OK |
| `Open` | `Cancelled` | 200 OK |
| `InProgress` | `Resolved` | 200 OK |
| `InProgress` | `Cancelled` | 200 OK |
| `Resolved` | `Closed` | 200 OK |

#### Invalid transitions — `UpdateTicket_RejectsInvalidStatusTransitions`

| From | To | Expected |
|------|----|----------|
| `Open` | `Resolved` | 400, error on `status` |
| `Open` | `Closed` | 400, error on `status` |
| `InProgress` | `Open` | 400, error on `status` |
| `Resolved` | `InProgress` | 400, error on `status` |
| `Resolved` | `Cancelled` | 400, error on `status` |
| `Closed` | `Open` | 400, error on `status` |

### 4.3 Test Setup — Status Path Builder

Tests that start from a non-`Open` status use a **BFS path builder** (`BuildStatusPath`) to reach the target status through valid intermediate steps:

```
Open → InProgress → Resolved   (to test Resolved → Closed)
Open → Cancelled               (to test Closed → Open rejection)
```

This ensures the ticket is in the correct starting state without bypassing workflow rules.

### 4.4 Assertions for Workflow Tests

| Check | Valid Transition | Invalid Transition |
|-------|-----------------|-------------------|
| HTTP status | `200 OK` | `400 Bad Request` |
| Response body | `ApiResponse<TicketDto>`, status matches | `ApiErrorResponse` |
| Error field | — | `errors` contains key `"status"` |
| Error message | — | Lists allowed transitions (implicit via key presence) |

### 4.5 Planned Workflow Tests

| Test | Scenario |
|------|----------|
| `UpdateTicket_SameStatus_Returns200` | No-op transition (status unchanged) |
| `UpdateTicket_FromCancelled_Returns400` | Terminal state rejection |
| `UpdateTicket_FromClosed_Returns400` | Terminal state rejection |

---

## 5. Validation Testing

### 5.1 Strategy

Validation is tested **indirectly through the API** — send invalid payloads and assert `400 Bad Request` with field-level errors in `ApiErrorResponse`. This verifies FluentValidation, `ValidationExtensions`, and `ApiBehaviorExtensions` together.

### 5.2 Validation Rules to Test

#### Ticket creation (`POST /api/tickets`)

| Field | Invalid Input | Expected Error Key |
|-------|---------------|-------------------|
| `title` | Empty / missing | `title` |
| `title` | > 200 characters | `title` |
| `description` | Empty / missing | `description` |
| `description` | > 4000 characters | `description` |
| `priority` | Invalid enum value | `priority` |
| `createdByUserId` | 0 or negative | `createdByUserId` |
| `createdByUserId` | Agent ID (not Customer) | `createdByUserId` |
| `createdByUserId` | Non-existent user | `createdByUserId` |
| `assignedToUserId` | 0 | `assignedToUserId` |
| `assignedToUserId` | Customer ID | `assignedToUserId` |
| `assignedToUserId` | Non-existent user | `assignedToUserId` |

#### Ticket update (`PUT /api/tickets/{id}`)

| Field | Invalid Input | Expected Error Key |
|-------|---------------|-------------------|
| `status` | Invalid transition | `status` |
| `status` | Invalid enum value | `status` |
| Body | Null / missing | `request body` or model-binding error |

#### Search (`GET /api/tickets`)

| Parameter | Invalid Input | Expected |
|-----------|---------------|----------|
| `pageNumber` | 0 or negative | 400 |
| `pageSize` | 0, 101, or negative | 400 |
| `pageNumber` | Exceeds `MaxPageNumber` | 400 |
| `status` | Invalid enum string | 400 (model binding) |
| `keyword` | > 200 characters | 400 |

#### Comments (`POST /api/tickets/{ticketId}/comments`)

| Field | Invalid Input | Expected |
|-------|---------------|----------|
| `content` | Empty | 400, key `content` |
| `content` | > 2000 characters | 400, key `content` |
| `userId` | 0 or non-existent | 400, key `userId` |
| `ticketId` | Closed/cancelled ticket | 400, key `ticketId` |
| `ticketId` | Non-existent ticket | 404 |

### 5.3 Error Shape Assertions

All validation tests should verify:

```csharp
Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

var body = await response.ReadAsJsonAsync<ApiErrorResponse>();
Assert.NotNull(body);
Assert.Equal(400, body.StatusCode);
Assert.NotNull(body.Errors);
Assert.True(body.Errors.ContainsKey("expectedFieldName"));
Assert.NotEmpty(body.Errors["expectedFieldName"]);
```

### 5.4 Current Validation Coverage

| Area | Covered | Notes |
|------|---------|-------|
| Invalid status transitions | ✅ | 7 theory cases with `status` key assertion |
| Required fields | — | Planned |
| Max length violations | — | Planned |
| Invalid user references | — | Planned |
| Pagination bounds | — | Planned |
| Model-binding errors | — | Planned (malformed JSON, wrong types) |
| Comment validation | — | Planned |

### 5.5 Recommended Test Class Structure

```
tests/
├── TicketsApiTests.cs              # CRUD, search, workflow (existing)
├── TicketValidationTests.cs        # Planned — create/update/search validation
├── CommentApiTests.cs              # Planned — comment CRUD
└── CommentValidationTests.cs       # Planned — comment validation + 404
```

---

## 6. Edge Cases

### 6.1 Edge Case Test Matrix

| Category | Scenario | Expected | Covered |
|----------|----------|----------|---------|
| **Existence** | GET ticket with non-existent ID | 404 | Planned |
| **Existence** | DELETE non-existent ticket | 404 | Planned |
| **Existence** | GET comments for non-existent ticket | 404 | Planned |
| **Existence** | POST comment on non-existent ticket | 404 | Planned |
| **Workflow** | Update to same status (no change) | 200 | Planned |
| **Workflow** | Transition from terminal state | 400 | Partial (Closed→Open only) |
| **Workflow** | Skip step (Open→Resolved) | 400 | ✅ |
| **Comments** | Comment on closed ticket | 400 | Planned |
| **Comments** | Comment on cancelled ticket | 400 | Planned |
| **Users** | Assign ticket to customer | 400 | Planned |
| **Users** | Create ticket with agent as creator | 400 | Planned |
| **Users** | Inactive user reference | 400 | Planned |
| **Pagination** | Page beyond available data | 200, empty items | Planned |
| **Pagination** | `pageSize` at boundary (1, 100) | 200 | Planned |
| **Pagination** | `pageSize` = 101 | 400 | Planned |
| **Search** | Empty keyword (no filter) | 200, all tickets | Partial (implicit in search test) |
| **Search** | Keyword with no matches | 200, `totalCount = 0` | Planned |
| **Payload** | Null request body on POST | 400 | Planned |
| **Payload** | Malformed JSON | 400, `ApiErrorResponse` | Planned |
| **Payload** | Integer enum in JSON body | 400 or accepted + validated | Planned |
| **Concurrency** | User deleted between validation and save | 409 | Planned (hard to reproduce in InMemory) |
| **Routing** | Non-integer ticket ID in route | 404 (routing) | Planned |

### 6.2 HTTP Status Code Verification

Tests must assert the **correct status code** for each scenario, not just success/failure:

| Status | Meaning | When |
|--------|---------|------|
| `200` | OK | Successful GET, PUT, search |
| `201` | Created | Successful POST |
| `204` | No Content | Successful DELETE |
| `400` | Bad Request | Validation failure |
| `404` | Not Found | Missing ticket |
| `409` | Conflict | FK violation / concurrency (planned) |
| `500` | Server Error | Must never occur in happy/edge paths |

### 6.3 Test Isolation

| Concern | Mitigation |
|---------|------------|
| Shared database state | Unique InMemory database per `WebApplicationFactory` instance |
| Seed data interference | Tests create their own tickets with distinct titles |
| Search test pollution | Keyword search uses specific terms ("login", "printer") unlikely to match seed titles |
| Ordering dependency | Tests do not depend on execution order (`IClassFixture` shares factory, not test sequence) |

### 6.4 Known Limitations

| Limitation | Impact | Mitigation |
|------------|--------|------------|
| InMemory provider ≠ SQL Server | FK behaviors and `IDENTITY_INSERT` differ | Add smoke tests against LocalDB in CI (future) |
| No concurrency simulation | 409 Conflict path untested | Manual or dedicated SQL Server test (future) |
| No auth testing | Endpoints are public | Add auth integration tests when JWT is implemented |
| Single test class | Comment APIs untested | Add `CommentApiTests` in v1.1 |

---

## 7. Test Execution in CI

### Recommended Pipeline

```yaml
steps:
  - dotnet restore
  - dotnet build --configuration Release
  - dotnet test tests/SupportTicketManagementSystem.IntegrationTests
        --configuration Release
        --no-build
        --logger "trx;LogFileName=test-results.trx"
        --collect:"XPlat Code Coverage"
```

### Quality Gates

| Gate | Threshold |
|------|-----------|
| Build | Zero errors |
| Tests | 100% pass rate |
| New features | At least one integration test per endpoint |
| Bug fixes | Regression test required |

---

## 8. Coverage Roadmap

| Phase | Tests to Add | Priority |
|-------|-------------|----------|
| **v1.1** | Comment API, 404 cases, DELETE 204 | High |
| **v1.1** | Validation theory tests (required fields, max length, user roles) | High |
| **v1.1** | Model-binding error shape (`ApiErrorResponse`) | Medium |
| **v1.2** | Unit tests for `TicketStatusWorkflow`, validators | Medium |
| **v2.0** | Auth integration tests (JWT, role-based access) | High |
| **v2.0** | SQL Server-backed smoke tests in CI | Low |

---

## Related Documents

- [design-notes.md](./design-notes.md) — Architecture and validation design
- [implementation-plan.md](./implementation-plan.md) — Milestones and testing plan
- [requirements-analysis.md](./requirements-analysis.md) — Acceptance criteria
- [README.md](./README.md) — Run and test commands
