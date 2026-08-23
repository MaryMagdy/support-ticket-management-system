# Screenshots / demo video

Every required scenario below was walked through live in a real browser session against the
running app (not mocked) as part of building this submission. The screenshots weren't saved
as files automatically — save the ones you want from the chat session (or re-run the steps
below yourself) into `docs/screenshots/` and reference them from the root `README.md`.

## Scenarios covered (all verified working)

1. **Login** — sign-in form, tested with all three roles.
2. **Admin dashboard** — total/status counts, open+critical count, average resolution time,
   "Tickets by status" bar chart, agent workload.
3. **Ticket list (Admin)** — search box, Status/Priority filters, sortable columns, pagination
   footer ("1–10 of 10" etc).
4. **Ticket detail — Details tab** — Admin sees editable Status/Priority selects plus an
   "Assigned agent" dropdown (Admin-only); Agent/Customer see read-only fields, and a Customer
   additionally sees a **"Close ticket"** button once the ticket is Resolved.
5. **Ticket detail — Comments tab** — existing thread + add-comment form.
6. **Ticket detail — Activity tab** — real timeline entries (assignment/priority/status
   changes) pulled from `GET /api/tickets/:id/activity`.
7. **Ticket detail — Time Tracking tab** — logged entries, auto-calculated total, log-time form.
8. **Admin Users page** — list/create/edit/delete users, client-side paginated.
9. **Customer "My Tickets"** — scoped to only the logged-in customer's own tickets (4 of the
   10 seeded tickets, for `customer1`).
10. **Create ticket (Customer)** — title/description/priority reactive form.
11. **Agent "My Assigned Tickets"** — scoped to only tickets assigned to that agent (2 of 10,
    for `agent1`).
12. **Data isolation proof** — logged in as `customer1`, navigated directly to
    `/tickets/5` (a ticket owned by `customer2`) by URL manipulation. Backend returned
    **404** on `GET /api/tickets/5` (not 403 — it doesn't even reveal the ticket exists), and
    the frontend renders a clear "Ticket not found" state rather than a blank page or leaking
    data.
13. **Customer closes a Resolved ticket** — logged in as the ticket's own customer, clicked
    "Close ticket" on a Resolved ticket, confirmed status flips to Closed and the button
    disappears (network trace: `PUT /api/tickets/:id` → 200).

## How to reproduce / capture your own

```bash
# terminal 1
cd backend && dotnet run --project src/SupportTickets.Api

# terminal 2
cd frontend && npm start
```

Open `http://localhost:4200`, log in with the seeded accounts (see root `README.md`), and
walk through the scenarios above. Seeded ticket IDs 1–10 exist out of the box; ticket #3 and
#5 are good ones to use for the activity-timeline and data-isolation demos respectively
(owned by `customer3` and `customer2`).
