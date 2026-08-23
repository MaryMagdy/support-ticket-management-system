# Support Ticket Management System

A full-stack support ticket management system built for the technical assessment: ASP.NET
Core 8 Web API backend + Angular 17 frontend, with JWT auth, role-based access control,
strict customer data isolation, ticket workflow, comments/activity timeline, time tracking,
and an analytics dashboard.

```
.
├── backend/    ASP.NET Core 8 Web API (see backend/README-BACKEND.md for full detail)
├── frontend/   Angular 17 SPA (see frontend/README-FRONTEND.md for full detail)
└── docs/       Screenshots / demo assets (see docs/README.md)
```

## Quick start

You need the **.NET 8 SDK** and **Node.js 18+** installed.

### 1. Backend (http://localhost:5000)

```bash
cd backend
dotnet restore
dotnet run --project src/SupportTickets.Api
```

- Runs against **SQLite** by default in Development (zero setup — a `dev.db` file is created
  automatically next to the API project) and auto-applies EF Core migrations + seed data on
  first run. See [`backend/README-BACKEND.md`](backend/README-BACKEND.md) for the SQL Server
  toggle.
- Swagger UI: the console prints the exact URL on startup, e.g. `http://localhost:5000/swagger`.

### 2. Frontend (http://localhost:4200)

```bash
cd frontend
npm install
npm start
```

Expects the API at `http://localhost:5000/api` (see `frontend/src/environments/environment.development.ts`).

### 3. Log in with a seeded test account

| Role         | Email                              | Password       |
|--------------|-------------------------------------|----------------|
| Admin        | admin@supporttickets.local          | `Admin123!`    |
| SupportAgent | agent1@supporttickets.local         | `Agent123!`    |
| SupportAgent | agent2@supporttickets.local         | `Agent123!`    |
| Customer     | customer1@supporttickets.local      | `Customer123!` |
| Customer     | customer2@supporttickets.local      | `Customer123!` |
| Customer     | customer3@supporttickets.local      | `Customer123!` |

## Running the tests

```bash
# Backend — 36/36 passing (24 unit + 12 integration, incl. data-isolation tests)
cd backend
dotnet test

# Frontend — 21/21 passing
cd frontend
npx ng test --watch=false --browsers=ChromeHeadless
```

## Architecture overview

**Backend** — clean layering: `Domain` (entities, enums, pure business rules) →
`Application` (DTOs, service interfaces, FluentValidation validators, exceptions) →
`Infrastructure` (EF Core, JWT/password hashing, service implementations, seeding) →
`Api` (controllers, middleware, composition root). Controllers only see `Application`
interfaces; EF entities are never exposed. A single exception-handling middleware converts
thrown exceptions to a consistent JSON error shape. Data isolation (a Customer can only ever
see their own tickets, an Agent only tickets assigned to them) is enforced **in the service
layer** on every single-ticket read/write — not just via list filtering — so IDOR-style ID
guessing returns 404, verified by dedicated integration tests.

**Frontend** — Angular 17 standalone components with lazy-loaded, role-gated routes
(`admin/`, `agent/`, `customer/`), a `core/` layer of typed services/models/guards/interceptors
mirroring the backend contract 1:1, and Angular Material for the UI. An HTTP interceptor
attaches the JWT and transparently retries once via refresh-token rotation on 401.

Full detail, including the exact API surface and the verified request/response contract, is
in [`backend/README-BACKEND.md`](backend/README-BACKEND.md) and
[`frontend/README-FRONTEND.md`](frontend/README-FRONTEND.md).

## Assumptions & known limitations

- **Database provider**: only a SQLite EF Core migration is checked in (used for local
  runs/tests — zero setup). Switching to SQL Server for a real deployment requires generating
  a second, SQL-Server-specific migration set (documented in `backend/README-BACKEND.md`).
- **"Open + critical" dashboard metric** is interpreted as *tickets still requiring attention*:
  any ticket that is `Open`, or not yet `Closed` and `Critical` priority.
- **Status transitions**: `Open → InProgress → Resolved → Closed`. Agents/Admins drive
  `Open→InProgress` and `InProgress→Resolved`; Customers or Admins drive `Resolved→Closed`.
  Any other jump is rejected (400) for non-admins; Admins can override any transition.
- **No email delivery, no rate limiting/account lockout, no "logout everywhere" endpoint** —
  out of scope for this assessment; refresh-token rotation covers the stated bonus item instead.
- **Agent/customer ticket lists** rely on server-side role scoping of `GET /api/tickets`
  (no client-side `assignedAgentId`/`customerId` filter is sent) — verified working via the
  backend's own data-isolation tests and a manual end-to-end smoke test.
- Styling is Angular Material defaults + light custom layout — a functional scaffold, not a
  final visual design pass.
- This was built collaboratively by Claude Code with heavy background-agent parallelization
  (backend + frontend built concurrently, then reconciled against the real API contract and
  smoke-tested end-to-end in a browser) rather than hand-written top to bottom; see git history
  for the shape of that process.

## Bonus items implemented

- **Refresh token rotation**: `POST /api/auth/refresh` issues a new token pair and immediately
  revokes the old refresh token; reuse of a revoked token is rejected (401), verified by an
  integration test.

## Deliverables checklist

- [x] Git repository (see below for pushing to a remote)
- [x] Database migrations + seeded test accounts for each role
- [x] This README (setup, test accounts, architecture, assumptions)
- [x] Backend Postman collection: [`backend/postman_collection.json`](backend/postman_collection.json)
      (Swagger/OpenAPI is also live at `/swagger` while the API is running)
- [ ] Screenshots / demo video — see [`docs/README.md`](docs/README.md) for what's needed and how to capture it
