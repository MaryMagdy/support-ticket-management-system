# Support Ticket Management System — Frontend

Angular 17 (standalone components, `provideRouter` + lazy routes) frontend built against a REST
contract. No live backend was available at build time — all services are typed and call the
endpoints described below, but there is nothing running at `http://localhost:5000` yet.

## Setup

```bash
npm install
```

## Run the dev server

```bash
npm start
# or
ng serve
```

Serves at `http://localhost:4200`. It expects an API at `http://localhost:5000/api`
(see `src/environments/environment.development.ts`).

## Build

```bash
npx ng build
```

**Result observed:** build succeeds (`Application bundle generation complete`). There is one
non-fatal warning: the initial bundle (766 KB raw / ~168 KB gzipped) exceeds the default 500 KB
`ng build` budget defined in `angular.json`, mainly because of Angular Material + Chart.js. This
is a warning, not an error — build exit code is 0. If this needs to be silenced, raise the
`budgets` thresholds in `angular.json` or lazy-load Chart.js further.

## Test

```bash
npx ng test --watch=false --browsers=ChromeHeadless
```

**Result observed:** ran successfully in this environment (Chrome was available) — **21/21 specs
passed**. Spec files included:

- `src/app/core/services/auth.service.spec.ts` — login success sets token/current-user state,
  logout clears it (uses `HttpClientTestingModule`).
- `src/app/core/services/ticket.service.spec.ts` — `getTickets` builds the correct URL and query
  params (page, pageSize, status, priority, search, sortBy, sortDir) via
  `HttpTestingController`; also covers `getTicket` and `createTicket`.
- `src/app/core/guards/auth.guard.spec.ts` — redirects to `/login` when logged out, allows
  navigation when logged in.
- `src/app/core/utils/ticket.utils.spec.ts` — ticket-list search filtering and the
  status-transition validation rules (`isValidStatusTransition`) used to gate status changes.
- `src/app/app.component.spec.ts` — basic smoke test.

If Chrome/ChromeHeadless is not installed in a given sandbox, this command fails with a launcher
error (`No binary for Chrome browser on your platform`) — that's an environment limitation, not a
code defect; the specs themselves are correct.

## Project structure

```
src/
  environments/            apiUrl config (dev + prod)
  app/
    core/
      models/              Ticket, Comment, TimeEntry, User, PagedResult<T>, DashboardSummary,
                            AuthResponse, LoginRequest, RegisterRequest, enums, ActivityLogEntry
      services/             AuthService, TicketService, CommentService, TimeEntryService,
                            UserService, DashboardService, LoadingService
      interceptors/         authInterceptor, errorInterceptor, loadingInterceptor
      guards/                authGuard, roleGuard
      utils/                ticket.utils.ts (status-transition + search-filter logic, unit-tested)
    shared/components/      spinner-overlay, confirm-dialog, page-header
    layout/shell/           Material sidenav + toolbar shell, role-based nav, responsive via
                            BreakpointObserver
    features/
      auth/                 login, register (reactive forms, password-match validator)
      admin/                dashboard (cards + ng2-charts bar chart + agent workload),
                            users (MatTable CRUD via dialogs)
      agent/                agent-tickets (assigned list, filter/search)
      customer/              customer-tickets (own list), create-ticket (reactive form)
      tickets/               ticket-list (admin, paginated/sortable/filterable MatTable),
                            ticket-detail (tabs: Details / Comments / Activity / Time Tracking)
      unauthorized/, not-found/
    app.routes.ts            root routing table, role-gated lazy children
    app.config.ts            provideRouter, provideAnimations, provideHttpClient(interceptors)
```

## Backend contract (verified against the real ASP.NET Core API)

- Base URL: `http://localhost:5000/api` (see `environment.ts` / `environment.development.ts`).
- All entity IDs are `number` (not string/guid). All enums (`TicketStatus`, `TicketPriority`,
  `UserRole`, `ActivityType`) serialize as **strings** (e.g. `"Open"`, `"SupportAgent"`,
  `"High"`) via a `JsonStringEnumConverter` on the backend, and the frontend enums mirror those
  string values exactly.
- Auth: JWT bearer token in `Authorization: Bearer <token>` header, plus a refresh token.
  - `POST /auth/login` `{ email, password }` → `AuthResponse { accessToken, refreshToken, expiresAt, user }`
  - `POST /auth/register` `{ fullName, email, password }` → `AuthResponse`
  - `POST /auth/refresh` `{ refreshToken }` → `AuthResponse`
  - On any `401` (other than from `/auth/*` itself), `authInterceptor` calls `/auth/refresh` once;
    if that also fails, the user is logged out and redirected to `/login`.
- Tickets:
  - `GET /tickets?page=&pageSize=&status=&priority=&assignedAgentId=&search=&sortBy=&descending=` →
    `PagedResult<TicketDto>` (`{ items, totalCount, page, pageSize }`). Note the sort param is
    `sortBy`/`descending` (bool), not `sortBy`/`sortDir`.
  - `GET /tickets/:id` → `TicketDto { id, title, description, status, priority, customerId,
    customerName, assignedAgentId, assignedAgentName, createdAt, updatedAt, totalTimeMinutes }`
  - `POST /tickets` `{ title, description, priority }` → `TicketDto`
  - `PUT /tickets/:id` `{ title?, description?, status?, priority?, assignedAgentId? }` → `TicketDto`
  - `DELETE /tickets/:id` (Admin only)
- Comments (nested under ticket, not a top-level resource):
  `GET/POST /tickets/:ticketId/comments`, `CommentDto { id, ticketId, userId, userName, text, createdAt }`,
  create payload `{ text }`.
- Time entries (nested under ticket, not a top-level resource):
  `GET/POST /tickets/:ticketId/timeentries`,
  `TimeEntryDto { id, ticketId, userId, userName, workDate, durationMinutes, description, createdAt }`,
  create payload `{ workDate, durationMinutes, description? }`.
- Users (Admin only): `GET /users` → plain `UserDto[]` (**not** a `PagedResult`).
- Dashboard: `GET /dashboard/summary` →
  `DashboardSummaryDto { totalTickets, countsByStatus: { [statusName]: number },
  openAndCriticalCount, averageResolutionTimeHours: number | null, agentWorkload: AgentWorkloadDto[] }`,
  `AgentWorkloadDto { agentId, agentName, openTicketCount }`.

This contract was verified directly against the running backend (`curl localhost:5000/api/...`)
and by reading the backend DTOs, replacing an earlier, incorrect best-effort guess.

## Known limitations / incomplete pieces

- **Optimistic concurrency**: `Ticket.rowVersion` (a Guid) is round-tripped on every status/
  priority/assignment update. If the ticket was changed by someone else since it was last
  fetched, the backend returns 409 and the global error interceptor shows the conflict message;
  `TicketDetailComponent` then reloads the ticket and activity log so the form reflects the
  current state before the user retries.
- **Ticket assignment**: the Details tab shows an "Assigned agent" dropdown for Admins only
  (populated from `GET /users?role=SupportAgent`), which calls `PUT /tickets/:id` with
  `assignedAgentId`. Agents/Customers see a read-only "Assigned to" line instead.
- **Admin ticket list vs. agent/customer lists** are three separate components
  (`TicketListComponent`, `AgentTicketsComponent`, `CustomerTicketsComponent`) rather than one
  parameterized component, to keep each one simple; they share the same `TicketService` calls.
  The agent/customer lists do not pass an `assignedAgentId`/`customerId` filter — the backend
  scopes `GET /tickets` results server-side based on the authenticated user's role (verified live).
- **`GET /api/users` is not paginated** — it returns the full array, so `UsersComponent` fetches
  it once and paginates client-side with `MatPaginator` rather than requesting pages from the
  server.
- **Bundle size** exceeds the default 500 KB Angular CLI budget (see Build section above) — build
  still succeeds, this is a warning only.
- **User creation UI** always requires a password field when creating (not editing) — matches a
  typical "admin sets initial password" flow; adjust if the backend instead emails an invite/reset
  link.
- Styling is intentionally minimal (Angular Material defaults + light custom layout) — this is a
  functional/technical-assessment scaffold, not a final visual design pass.
- TypeScript `strict` mode is enabled (CLI default for new projects) and the app builds cleanly
  under it.
