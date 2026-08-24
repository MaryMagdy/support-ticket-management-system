# Screenshots

Screenshots below were captured from a real, running instance of the app (backend + Angular
dev server), exercising every required end-to-end scenario. No mocked data or hand-edited
images — each PNG is a genuine browser screenshot from the flow described.

| # | Screenshot | Scenario |
|---|------------|----------|
| 1 | [`01-login-page.png`](screenshots/01-login-page.png) | Sign-in page |
| 2 | [`02-admin-dashboard.png`](screenshots/02-admin-dashboard.png) | Admin dashboard — counts by status, open+critical, avg resolution time, "Tickets by status" chart, agent workload |
| 3 | [`03-admin-ticket-list.png`](screenshots/03-admin-ticket-list.png) | Admin "All Tickets" — search, Status/Priority filters, sortable columns, pagination |
| 4 | [`04-admin-users.png`](screenshots/04-admin-users.png) | Admin "Users" management page |
| 5 | [`05-ticket-detail-details-tab.png`](screenshots/05-ticket-detail-details-tab.png) | Ticket detail — Details tab (Admin view: editable Status/Priority + Assigned agent) |
| 6 | [`06-ticket-detail-comments-tab.png`](screenshots/06-ticket-detail-comments-tab.png) | Ticket detail — Comments tab |
| 7 | [`07-ticket-detail-activity-tab.png`](screenshots/07-ticket-detail-activity-tab.png) | Ticket detail — Activity tab (real timeline from `GET /tickets/:id/activity`) |
| 8 | [`08-ticket-detail-timetracking-tab.png`](screenshots/08-ticket-detail-timetracking-tab.png) | Ticket detail — Time Tracking tab (logged entries + total + log-time form) |
| 9 | [`09-customer-ticket-list.png`](screenshots/09-customer-ticket-list.png) | Customer "My Tickets" — scoped to only that customer's own tickets |
| 10 | [`10-customer-create-ticket-form.png`](screenshots/10-customer-create-ticket-form.png) | Customer creating a new ticket (title/description/priority) |
| 11 | [`11-customer-after-create-ticket.png`](screenshots/11-customer-after-create-ticket.png) | The newly created ticket, auto-generated ID, `Open` status |
| 12 | [`12-admin-assign-ticket-to-agent.png`](screenshots/12-admin-assign-ticket-to-agent.png) | Admin assigns the new ticket to a Support Agent |
| 13 | [`13-agent-ticket-list.png`](screenshots/13-agent-ticket-list.png) | Agent "My Assigned Tickets" — scoped to only tickets assigned to that agent |
| 14 | [`14-agent-ticket-inprogress.png`](screenshots/14-agent-ticket-inprogress.png) | Agent moves the ticket to `InProgress` |
| 15 | [`15-agent-ticket-resolved.png`](screenshots/15-agent-ticket-resolved.png) | Agent moves the ticket to `Resolved` |
| 16 | [`16-customer-sees-resolved-close-button.png`](screenshots/16-customer-sees-resolved-close-button.png) | Customer sees the **"Close ticket"** button once their ticket is Resolved |
| 17 | [`17-customer-closed-ticket.png`](screenshots/17-customer-closed-ticket.png) | Customer closes it — status flips to `Closed`, button disappears |
| 18 | [`18-data-isolation-denied-other-customer-ticket.png`](screenshots/18-data-isolation-denied-other-customer-ticket.png) | **Data isolation proof**: a different customer navigates directly to that ticket's URL and is denied — "Ticket not found" (backend returns 404, not a data leak) |

## How to reproduce

```bash
# terminal 1
cd backend && dotnet run --project src/SupportTickets.Api

# terminal 2
cd frontend && npm start
```

Open `http://localhost:4200` and log in with the seeded accounts from the root `README.md`.
The full ticket lifecycle above (create → assign → in-progress → resolved → closed) and the
data-isolation check can be replayed with any two customer accounts and one agent account.
