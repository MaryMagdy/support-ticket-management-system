# Support Ticket Management System — Backend

ASP.NET Core 8 Web API implementing a Support Ticket Management System with role-based
access control (Admin / SupportAgent / Customer), JWT authentication with refresh-token
rotation, ticket workflow state machine, comments, time tracking, activity timeline, and
an analytics dashboard.

## Architecture

Clean layering, one project per layer:

```
backend/
  SupportTickets.sln
  src/
    SupportTickets.Domain          Entities, enums, pure business rules (status transitions, calculations)
    SupportTickets.Application     DTOs, service interfaces, FluentValidation validators, exceptions
    SupportTickets.Infrastructure  EF Core DbContext, migrations, service implementations, JWT/password hashing, seeding
    SupportTickets.Api             Controllers, middleware, Program.cs (composition root), Swagger
  tests/
    SupportTickets.UnitTests        xUnit — business rules, authorization/ownership checks (in-memory EF provider)
    SupportTickets.IntegrationTests xUnit + WebApplicationFactory — full HTTP pipeline against SQLite in-memory
```

Controllers depend only on `Application` interfaces (`ITicketService`, `IAuthService`, ...);
EF entities are never returned directly — everything is mapped to DTOs. A single
`ExceptionHandlingMiddleware` converts thrown exceptions (`NotFoundException`,
`ForbiddenException`, `ValidationAppException`, FluentValidation's `ValidationException`, etc.)
into a consistent JSON error shape:

```json
{ "status": 404, "message": "Ticket not found.", "errors": null, "traceId": "..." }
```

## Database provider toggle (SQLite ⇄ SQL Server)

Controlled by the `Database:Provider` configuration key, read in
`SupportTickets.Infrastructure/DependencyInjection.cs`:

- `src/SupportTickets.Api/appsettings.Development.json` → `"Database:Provider": "Sqlite"`,
  connection string `ConnectionStrings:Sqlite` (defaults to `Data Source=dev.db`, a file
  created next to the API project — **this is what reviewers should use, zero setup**).
- `src/SupportTickets.Api/appsettings.json` (used in Production, or whenever
  `ASPNETCORE_ENVIRONMENT` isn't `Development`) → `"Database:Provider": "SqlServer"`,
  connection string `ConnectionStrings:SqlServer` (LocalDB by default — edit for your own
  SQL Server instance).

To force a provider regardless of environment, override `Database:Provider` via
environment variable, e.g. `Database__Provider=SqlServer`.

Only one EF Core migration set exists (`src/SupportTickets.Infrastructure/Migrations`),
generated against SQLite, which is the default provider for local runs/tests. If you switch
to SQL Server you will need to generate a SQL Server-specific migration (see below) because
SQLite and SQL Server migrations are not always binary compatible (column types, etc).

## Running the API

```bash
cd backend
dotnet restore
dotnet build
dotnet run --project src/SupportTickets.Api
```

By default `ASPNETCORE_ENVIRONMENT=Development` (see `Properties/launchSettings.json`), so:

- The app runs EF Core migrations automatically on startup (`Database.MigrateAsync()`).
- Seed data is inserted automatically the first time (only if the `Users` table is empty).
- Swagger UI is enabled.

Swagger UI: **https://localhost:{port}/swagger** (the exact port is printed on startup, e.g.
`http://localhost:5168/swagger`). Click **Authorize** and paste `Bearer <access-token>` to
call protected endpoints from the UI.

## Running migrations manually

```bash
dotnet tool install --global dotnet-ef
cd backend/src/SupportTickets.Api
dotnet ef database update --project ../SupportTickets.Infrastructure --startup-project .
```

To add a new migration after changing entities:

```bash
dotnet ef migrations add <Name> --project ../SupportTickets.Infrastructure --startup-project . --output-dir ../SupportTickets.Infrastructure/Migrations
```

## Seed data / test accounts

Seeded automatically on first run in Development (1 Admin, 2 SupportAgents, 3 Customers,
10 tickets with comments, time entries and activity logs):

| Role         | Email                              | Password      |
|--------------|-------------------------------------|---------------|
| Admin        | admin@supporttickets.local          | `Admin123!`   |
| SupportAgent | agent1@supporttickets.local         | `Agent123!`   |
| SupportAgent | agent2@supporttickets.local         | `Agent123!`   |
| Customer     | customer1@supporttickets.local      | `Customer123!`|
| Customer     | customer2@supporttickets.local      | `Customer123!`|
| Customer     | customer3@supporttickets.local      | `Customer123!`|

Passwords are hashed with ASP.NET Core Identity's `PasswordHasher<T>`
(`Microsoft.Extensions.Identity.Core`), never stored in plaintext.

## Authentication

- `POST /api/auth/register` — self-register as a Customer.
- `POST /api/auth/login` — returns `{ accessToken, refreshToken, expiresAt, user }`.
- `POST /api/auth/refresh` — exchanges a valid refresh token for a new pair; the old refresh
  token is immediately revoked (rotation) and further use of it returns 401.
- Access tokens are JWT, HMAC-SHA256 signed, **30-minute** lifetime by default
  (`Jwt:AccessTokenMinutes`), refresh tokens are opaque random values stored in the
  `RefreshTokens` table with a **7-day** lifetime (`Jwt:RefreshTokenDays`).
- Send `Authorization: Bearer <accessToken>` on all protected endpoints.

## Authorization model

| Role         | Tickets                                             | Users             | Dashboard |
|--------------|------------------------------------------------------|-------------------|-----------|
| Admin        | Full CRUD, any ticket, assign agents, delete          | Full CRUD         | Yes       |
| SupportAgent | View/update only tickets assigned to them, no delete  | No access         | No        |
| Customer     | Create, view/update only their own tickets, no delete | No access         | No        |

Data isolation is enforced **server-side in the service layer** (not just via query
filtering) — `TicketService` checks ownership/assignment on every single-ticket
read/write and throws `NotFoundException` (404) if the ticket doesn't belong to the
caller, so a Customer cannot discover another customer's ticket even exists by
incrementing IDs. This is covered by dedicated integration tests
(`TicketDataIsolationTests`).

### Ticket status state machine

```
Open -> InProgress -> Resolved -> Closed
```

- `Open -> InProgress` and `InProgress -> Resolved`: SupportAgent (or Admin) only.
- `Resolved -> Closed`: Customer or Admin only.
- Any other jump (e.g. `Closed -> Open`) is rejected with **400** for non-admins.
- **Admin can override** any transition, including backward jumps, for corrections.

Implemented as a pure, easily-unit-tested static rule set:
`SupportTickets.Domain.Rules.TicketStatusRules.IsValidTransition(from, to, role)`.

## API surface

All endpoints are under `/api` and require `Authorization: Bearer <token>` unless noted.

- `POST /api/auth/register` *(anonymous)*
- `POST /api/auth/login` *(anonymous)*
- `POST /api/auth/refresh` *(anonymous)*
- `GET/POST/PUT/DELETE /api/users` *(Admin only)*
- `GET /api/tickets` — paginated (`page`, `pageSize`), filterable (`status`, `priority`,
  `assignedAgentId`), searchable (`search` — matches title/description), sortable
  (`sortBy` = title|priority|status|updatedAt|createdAt, `descending`)
- `GET /api/tickets/{id}`
- `POST /api/tickets` *(Customer or Admin)*
- `PUT /api/tickets/{id}` — status/priority/assignment transitions validated server-side
- `DELETE /api/tickets/{id}` *(Admin only)*
- `GET/POST /api/tickets/{ticketId}/comments`
- `GET /api/tickets/{ticketId}/timeentries`, `POST ...` *(SupportAgent/Admin only)*
- `GET /api/dashboard/summary` *(Admin only)* — counts by status, open+critical count,
  average resolution time in hours (`(UpdatedAt - CreatedAt)` averaged over
  Resolved/Closed tickets), agent workload (open ticket count per agent)

Full request/response examples: see [`postman_collection.json`](./postman_collection.json).

## CORS

Configured to allow `http://localhost:4200` (Angular dev server convention) with any
header/method.

## Logging

Structured console logging via the built-in `Microsoft.Extensions.Logging` console
provider. The exception middleware logs unhandled (500) exceptions at `Error` and all
other handled `AppException`s at `Warning`, including the request path and trace id.

## Running tests

```bash
cd backend
dotnet test
```

This runs:

- **SupportTickets.UnitTests** — ticket status transition rules, total-time-spent and
  average-resolution-time calculations, and service-level authorization/ownership checks
  (using EF Core's InMemory provider so no I/O is required).
- **SupportTickets.IntegrationTests** — full HTTP pipeline via `WebApplicationFactory`
  against a real SQLite database held open in-memory (`DataSource=:memory:`) for the
  duration of each test class; covers the register/login/refresh flow, ticket CRUD,
  validation (400s), and — critically — the **data isolation** tests proving Customer A
  cannot GET or PUT Customer B's ticket by guessing/incrementing its id (expects 403/404),
  cannot see it in their own ticket listing, and that anonymous requests are rejected (401).

Last known-good result: **36/36 tests passing** (24 unit + 12 integration).

## Known limitations / assumptions

- The single EF Core migration set targets SQLite only; switching to SQL Server in a real
  deployment requires generating a SQL-Server-specific migration (`dotnet ef migrations add
  InitialCreateSqlServer` with `Database:Provider=SqlServer`), since some SQLite/SQL Server
  column mappings differ.
- "Open + critical" on the dashboard is interpreted as *tickets still requiring attention*:
  any ticket that is `Open`, or is not yet `Closed` and has `Critical` priority.
- Refresh tokens are stored per-user in a simple table (no device/session metadata); a
  compromised refresh token is only stoppable by revocation via rotation misuse detection
  (using an already-revoked token fails), not by an explicit "logout everywhere" endpoint
  (not in scope of the spec).
- No email delivery is implemented; registration/reset flows are API-only.
- Rate limiting / account lockout after repeated failed logins is not implemented.
