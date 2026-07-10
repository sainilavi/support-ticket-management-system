# Requirements Analysis

**Project:** Support Ticket Management System  
**Version:** 1.0  
**Type:** ASP.NET Core 8 REST API (Backend)

---

## 1. Business Problem

Organizations handling customer support need a centralized system to log, track, and resolve customer issues. Without a structured ticket management process, support teams face:

- **Lost or duplicated requests** — Issues reported via email, chat, or phone are hard to track
- **No visibility into ticket status** — Customers and agents cannot see where an issue stands in the resolution pipeline
- **Inconsistent handling** — No enforced workflow for progressing tickets from open to closed
- **Poor communication history** — Updates and notes are scattered across channels

The **Support Ticket Management System** addresses this by providing a backend API that allows customers to raise tickets, agents to work and assign them, and all parties to follow a defined lifecycle with auditable comments.

### Target Users

| Role | Description |
|------|-------------|
| **Customer** | Creates tickets and adds comments on their issues |
| **Agent** | Works assigned tickets, updates status, and communicates via comments |
| **Admin** | Oversees tickets and may be assigned as a resolver |

---

## 2. Functional Requirements

### 2.1 Ticket Management

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-01 | The system shall allow creation of a support ticket with title, description, priority, creator, and optional assignee | Must |
| FR-02 | The system shall allow retrieval of a single ticket by ID, including creator and assignee display names | Must |
| FR-03 | The system shall allow updating of ticket title, description, status, priority, and assignee | Must |
| FR-04 | The system shall allow deletion of a ticket by ID | Must |
| FR-05 | New tickets shall be created with status **Open** by default | Must |
| FR-06 | The system shall return **404 Not Found** when a requested ticket does not exist | Must |

### 2.2 Ticket Status Workflow

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-07 | The system shall enforce the following status transitions: | Must |
| | `Open` → `InProgress`, `Cancelled` | |
| | `InProgress` → `Resolved`, `Cancelled` | |
| | `Resolved` → `Closed` | |
| FR-08 | `Closed` and `Cancelled` shall be terminal states (no further transitions) | Must |
| FR-09 | Invalid status transitions shall be rejected with a **400 Bad Request** and a field-level error on `status` | Must |
| FR-10 | Updating a ticket to its current status (no change) shall be permitted | Must |

### 2.3 Search and Pagination

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-11 | The system shall support searching tickets by keyword (matches title or description) | Must |
| FR-12 | The system shall support filtering tickets by status | Must |
| FR-13 | Search results shall be paginated with configurable `pageNumber` and `pageSize` | Must |
| FR-14 | Search results shall include total count, page metadata, and a `hasNextPage` indicator | Must |
| FR-15 | Results shall be ordered by creation date (newest first) | Must |

### 2.4 Comments

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-16 | The system shall allow adding a comment to an existing ticket | Must |
| FR-17 | The system shall allow retrieving all comments for a ticket, ordered by `CreatedAt` ascending | Must |
| FR-18 | Comments shall not be permitted on **Closed** or **Cancelled** tickets | Must |
| FR-19 | The system shall return **404 Not Found** when commenting on or listing comments for a non-existent ticket | Must |

### 2.5 User Validation

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-20 | Ticket creator (`CreatedByUserId`) must be an active user with role **Customer** | Must |
| FR-21 | Ticket assignee (`AssignedToUserId`), if provided, must be an active user with role **Agent** or **Admin** | Must |
| FR-22 | Comment author (`UserId`) must be an active user (any role) | Must |
| FR-23 | Assignee is optional; all other user references are required | Must |

### 2.6 Data Validation

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-24 | Title is required; maximum length **200** characters | Must |
| FR-25 | Description is required; maximum length **4000** characters | Must |
| FR-26 | Comment content is required; maximum length **2000** characters | Must |
| FR-27 | Priority must be a valid enum: `Low`, `Medium`, `High`, `Critical` | Must |
| FR-28 | Status must be a valid enum: `Open`, `InProgress`, `Resolved`, `Closed`, `Cancelled` | Must |
| FR-29 | Search keyword, if provided, must not exceed **200** characters | Must |
| FR-30 | Page number must be ≥ 1; page size must be between **1** and **100** | Must |

### 2.7 API Behavior

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-31 | All successful responses shall use a consistent `ApiResponse<T>` envelope | Must |
| FR-32 | All error responses shall use a consistent `ApiErrorResponse` envelope with field-level errors | Must |
| FR-33 | Validation failures shall return **400 Bad Request** | Must |
| FR-34 | Successful ticket deletion shall return **204 No Content** | Must |
| FR-35 | Interactive API documentation (Swagger) shall be available in Development | Should |

### 2.8 Data Seeding

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-36 | The system shall seed sample users, tickets, and comments in Development and Testing environments | Should |
| FR-37 | Seed operations shall be idempotent (no duplicate records on repeated runs) | Must |

---

## 3. Non-Functional Requirements

### 3.1 Architecture

| ID | Requirement |
|----|-------------|
| NFR-01 | The system shall follow **Clean Architecture** with Domain, Application, Infrastructure, and API layers |
| NFR-02 | Dependencies shall flow inward: API → Infrastructure → Application → Domain |
| NFR-03 | Data access shall use the **Repository** and **Unit of Work** patterns |
| NFR-04 | Business orchestration shall reside in the **Service** layer |

### 3.2 Technology

| ID | Requirement |
|----|-------------|
| NFR-05 | Runtime: **.NET 8** |
| NFR-06 | Database: **SQL Server** via **Entity Framework Core 8** |
| NFR-07 | Object mapping: **AutoMapper** |
| NFR-08 | Input validation: **FluentValidation** |
| NFR-09 | API documentation: **Swashbuckle (Swagger)** |

### 3.3 Performance

| ID | Requirement |
|----|-------------|
| NFR-10 | Read-only queries shall use no-tracking database access where appropriate |
| NFR-11 | Pagination shall limit result sets to a configurable page size (max 100) |
| NFR-12 | Post-save entity loading shall avoid full re-fetch of created/updated records |

### 3.4 Reliability & Error Handling

| ID | Requirement |
|----|-------------|
| NFR-13 | Unhandled exceptions shall be caught by global middleware and mapped to appropriate HTTP status codes |
| NFR-14 | Exceptions shall be logged with structured context (method, path, trace ID) |
| NFR-15 | Database concurrency and constraint violations shall return **409 Conflict**, not **500** |
| NFR-16 | Client disconnects shall not be logged as server errors |

### 3.5 Maintainability

| ID | Requirement |
|----|-------------|
| NFR-17 | Code shall follow consistent naming, folder structure, and error-handling conventions |
| NFR-18 | EF Core migrations shall manage schema versioning |
| NFR-19 | Integration tests shall verify critical API flows end-to-end |

### 3.6 Security (Current Scope & Gaps)

| ID | Requirement | Status |
|----|-------------|--------|
| NFR-20 | Input validation on all request payloads | Implemented |
| NFR-21 | Parameterized database access via EF Core | Implemented |
| NFR-22 | Authentication and authorization (JWT / role-based access) | **Not implemented** |
| NFR-23 | User identity derived from security claims, not request body | **Not implemented** |

---

## 4. Assumptions

| ID | Assumption |
|----|------------|
| A-01 | The API is consumed by a separate frontend or integration client; no UI is in scope |
| A-02 | User accounts are pre-provisioned; there is no self-registration endpoint |
| A-03 | The caller supplies `CreatedByUserId` and `UserId` in request bodies until authentication is added |
| A-04 | SQL Server (or LocalDB) is available for Development and production deployments |
| A-05 | A single organization tenant is supported; multi-tenancy is out of scope |
| A-06 | Tickets are soft-independent entities; deleting a ticket removes it permanently (hard delete) |
| A-07 | All timestamps are stored and compared in UTC |
| A-08 | English is the only supported language for validation messages and API responses |
| A-09 | Comment listing returns all comments for a ticket (no comment-level pagination in v1) |
| A-10 | One assignee per ticket is supported; multi-assignee is out of scope |

---

## 5. Edge Cases

### 5.1 Ticket Lifecycle

| Scenario | Expected Behavior |
|----------|-------------------|
| Update ticket to same status | Allowed (no-op transition) |
| Skip workflow step (e.g., `Open` → `Resolved`) | Rejected with **400** and status field error |
| Transition from `Closed` or `Cancelled` | Rejected; terminal states have no allowed transitions |
| Update non-existent ticket | **404 Not Found** |
| Delete non-existent ticket | **404 Not Found** |

### 5.2 Comments

| Scenario | Expected Behavior |
|----------|-------------------|
| Add comment to `Closed` ticket | Rejected with **400** |
| Add comment to `Cancelled` ticket | Rejected with **400** |
| Add comment to non-existent ticket | **404 Not Found** |
| List comments for non-existent ticket | **404 Not Found** |
| Comment with inactive user | Rejected with **400** |
| Ticket deleted between existence check and save | **409 Conflict** |

### 5.3 User References

| Scenario | Expected Behavior |
|----------|-------------------|
| `CreatedByUserId` is not a Customer | Rejected with **400** |
| `AssignedToUserId` is a Customer | Rejected with **400** |
| `AssignedToUserId` is 0 or negative | Rejected with **400** (must be greater than 0) |
| `AssignedToUserId` is null | Allowed (unassigned ticket) |
| Referenced user is inactive | Rejected with **400** |
| User deleted between validation and save | **409 Conflict** |

### 5.4 Search and Pagination

| Scenario | Expected Behavior |
|----------|-------------------|
| Empty keyword | Returns all tickets (subject to status filter) |
| No matching keyword | Returns empty items with `totalCount = 0` |
| `pageNumber` beyond available data | Returns empty items with valid metadata |
| Invalid enum in query string | **400** with consistent error envelope |
| Extremely large `pageNumber` | Rejected by validation (overflow protection) |
| `pageSize` > 100 | Rejected with **400** |

### 5.5 Request Payloads

| Scenario | Expected Behavior |
|----------|-------------------|
| Null or missing request body on POST/PUT | **400** with validation error |
| Empty title or description | **400** with field-level errors |
| Title/description/comment exceeding max length | **400** with max-length message |
| Invalid JSON or type mismatch | **400** with consistent `ApiErrorResponse` |
| Malformed ticket ID in route (non-integer) | Routing **404** (no controller match) |

---

## 6. Acceptance Criteria Summary

### Ticket CRUD

- [ ] `POST /api/tickets` creates a ticket with status `Open` and returns **201 Created**
- [ ] `GET /api/tickets/{id}` returns the ticket or **404** if not found
- [ ] `PUT /api/tickets/{id}` updates the ticket and returns **200 OK**
- [ ] `DELETE /api/tickets/{id}` removes the ticket and returns **204 No Content**
- [ ] Invalid payloads return **400** with field-level errors in `ApiErrorResponse`

### Status Workflow

- [ ] Valid transitions (`Open→InProgress`, `InProgress→Resolved`, `Resolved→Closed`, `Open→Cancelled`, `InProgress→Cancelled`) succeed
- [ ] All other transitions return **400** with an error on the `status` field
- [ ] Error message lists allowed transitions from the current status

### Search

- [ ] `GET /api/tickets?keyword=...` filters by title and description (case-insensitive)
- [ ] `GET /api/tickets?status=Open` filters by status
- [ ] `GET /api/tickets?pageNumber=1&pageSize=10` returns paginated results with metadata
- [ ] Invalid pagination parameters return **400**

### Comments

- [ ] `POST /api/tickets/{ticketId}/comments` adds a comment and returns **201 Created**
- [ ] `GET /api/tickets/{ticketId}/comments` returns comments ordered oldest-first
- [ ] Comments on closed/cancelled tickets are rejected with **400**
- [ ] Operations on non-existent tickets return **404**

### Cross-Cutting

- [ ] All validation errors use the `ApiErrorResponse` format (including model-binding failures)
- [ ] Swagger UI is accessible in Development at `/swagger`
- [ ] Seed data loads without duplicates on repeated application starts
- [ ] Integration test suite passes (`dotnet test`)

---

## 7. Out of Scope (v1)

The following are explicitly excluded from the current release:

- User registration, login, and JWT authentication
- Role-based endpoint authorization
- Email or push notifications
- File attachments on tickets or comments
- Ticket categories, tags, or SLA tracking
- Comment pagination
- Multi-tenancy
- Admin dashboard or reporting UI

These items are documented as future improvements in [README.md](./README.md).

---

## Related Documents

- [README.md](./README.md) — Setup, API reference, and run instructions
- [tool-workflow.md](./tool-workflow.md) — Cursor AI development workflow
