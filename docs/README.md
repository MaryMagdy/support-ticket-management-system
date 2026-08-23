# Screenshots / demo video

The assessment asks for screenshots or a short demo video showing the working app. This
wasn't captured automatically as part of the build — it needs a human (or a screen recorder)
driving the running app, ideally covering:

1. Login as each role (Admin / SupportAgent / Customer)
2. Admin dashboard (ticket counts, chart, agent workload)
3. Ticket list — filter, search, sort, pagination
4. Ticket detail — Details / Comments / Activity / Time Tracking tabs
5. Creating a ticket as a Customer, then an Agent picking it up and resolving it
6. A blocked action proving data isolation (e.g. a Customer trying to open another
   customer's ticket URL directly and getting redirected/blocked)

## How to capture

```bash
# terminal 1
cd backend && dotnet run --project src/SupportTickets.Api

# terminal 2
cd frontend && npm start
```

Then open `http://localhost:4200`, log in with the seeded accounts (see root `README.md`),
and record a short screen capture (OBS, Xbox Game Bar `Win+G`, or similar) or take
screenshots at each step above. Save them into this `docs/` folder (e.g. `docs/screenshots/`)
or link an uploaded video, then reference them from the root `README.md`.
