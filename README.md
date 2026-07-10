# Support Ticket Management System

A RESTful backend API for managing support tickets, built with ASP.NET Core 8 and Clean Architecture. The system supports ticket lifecycle management, threaded comments, search with pagination, and structured validation and error handling.

## Features

- **Ticket management** — Create, read, update, delete, and search tickets
- **Status workflow** — Enforced transitions: `Open` → `InProgress` / `Cancelled` → `Resolved` → `Closed`
- **Comments** — Add and retrieve comments per ticket (blocked on closed/cancelled tickets)
- **Search & pagination** — Filter by keyword and status with paginated results
- **Validation** — FluentValidation for request and business-rule validation
- **Consistent API responses** — Standardized success (`ApiResponse<T>`) and error (`ApiErrorResponse`) envelopes
- **Global exception handling** — Centralized middleware with structured logging
- **Swagger documentation** — Interactive API docs with grouped endpoints and examples
- **Seed data** — Sample users, tickets, and comments for local development
- **Integration tests** — End-to-end API tests using xUnit and `WebApplicationFactory`

## Tech Stack

| Layer | Technology |
|-------|------------|
| Runtime | .NET 8 |
| API | ASP.NET Core Web API |
| ORM | Entity Framework Core 8.0.11 |
| Database | SQL Server |
| Mapping | AutoMapper 12 |
| Validation | FluentValidation 11 |
| API Docs | Swashbuckle (Swagger) 6.6 |
| Testing | xUnit, ASP.NET Core Test Host, EF Core InMemory |

## Project Structure

```
SupportTicketManagementSystem/
├── src/
│   ├── SupportTicketManagementSystem.Domain/          # Entities, enums, domain rules
│   ├── SupportTicketManagementSystem.Application/     # DTOs, services, validators, interfaces
│   ├── SupportTicketManagementSystem.Infrastructure/  # EF Core, repositories, migrations, seeding
│   └── SupportTicketManagementSystem.API/             # Controllers, middleware, Swagger, startup
├── tests/
│   └── SupportTicketManagementSystem.IntegrationTests/
├── SupportTicketManagementSystem.slnx
└── dotnet-tools.json
```

**Dependency flow:** `Domain` ← `Application` ← `Infrastructure` ← `API`

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server or [SQL Server LocalDB](https://learn.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb) (included with Visual Studio)
- (Optional) [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

## SQL Server Setup

The default connection string in `src/SupportTicketManagementSystem.API/appsettings.json` targets LocalDB:

```
Server=(localdb)\mssqllocaldb;Database=SupportTicketManagementSystemDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
```

To use a different SQL Server instance, update `ConnectionStrings:DefaultConnection` in `appsettings.json` or override via user secrets / environment variables.

**Example (SQL Server Express):**

```json
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=SupportTicketManagementSystemDb;Trusted_Connection=True;TrustServerCertificate=True"
```

## EF Core Migration Commands

Install the EF Core CLI tool (already defined in `dotnet-tools.json`):

```bash
dotnet tool restore
```

**Apply migrations** (creates/updates the database):

```bash
dotnet ef database update \
  --project src/SupportTicketManagementSystem.Infrastructure \
  --startup-project src/SupportTicketManagementSystem.API
```

**Add a new migration** (after model changes):

```bash
dotnet ef migrations add <MigrationName> \
  --project src/SupportTicketManagementSystem.Infrastructure \
  --startup-project src/SupportTicketManagementSystem.API \
  --output-dir Data/Migrations
```

> In **Development**, migrations are applied automatically on startup. Seed data is loaded after migration.

## Run Instructions

**Restore, build, and run:**

```bash
dotnet restore
dotnet build
dotnet run --project src/SupportTicketManagementSystem.API
```

**Default URLs (HTTPS profile):**

| Endpoint | URL |
|----------|-----|
| API | `https://localhost:7263` |
| Swagger UI | `https://localhost:7263/swagger` |
| HTTP (alternate) | `http://localhost:5247` |

## API Documentation

Swagger UI is available in **Development** at `/swagger`. Endpoints are grouped into **Tickets** and **Comments**.

### Tickets (`/api/tickets`)

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/tickets` | Search tickets (`keyword`, `status`, `pageNumber`, `pageSize`) |
| `GET` | `/api/tickets/{id}` | Get ticket by ID |
| `POST` | `/api/tickets` | Create a ticket |
| `PUT` | `/api/tickets/{id}` | Update a ticket |
| `DELETE` | `/api/tickets/{id}` | Delete a ticket |

### Comments (`/api/tickets/{ticketId}/comments`)

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/tickets/{ticketId}/comments` | List comments for a ticket |
| `POST` | `/api/tickets/{ticketId}/comments` | Add a comment to a ticket |

### Response Format

**Success:**

```json
{
  "success": true,
  "data": { },
  "message": "Optional message"
}
```

**Error:**

```json
{
  "success": false,
  "statusCode": 400,
  "message": "One or more validation errors occurred.",
  "errors": { "title": ["Title is required."] },
  "traceId": "00-..."
}
```

### Seed Users

| ID | Role | Email |
|----|------|-------|
| 1 | Admin | admin@support.com |
| 2 | Agent | agent@support.com |
| 3 | Customer | customer@support.com |

## Test Execution

Run all integration tests:

```bash
dotnet test tests/SupportTicketManagementSystem.IntegrationTests
```

Tests use an in-memory database and do not require SQL Server. Coverage includes ticket CRUD, status transitions, search/pagination, and validation scenarios.

## Future Improvements

- **Authentication & authorization** — JWT or Identity with role-based access (Admin, Agent, Customer)
- **User context from claims** — Derive `CreatedByUserId` / `UserId` from the authenticated user instead of the request body
- **Comment pagination** — Paginate comments on tickets with large thread volumes
- **AutoMapper upgrade** — Move to AutoMapper 15.1.3+ to address known security advisories
- **Rate limiting & API versioning** — Protect public endpoints and support backward-compatible API evolution
- **Health checks & observability** — Add `/health` endpoints and structured application metrics

## License

This project is provided for educational and demonstration purposes.
