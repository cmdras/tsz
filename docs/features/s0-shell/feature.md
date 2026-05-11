# S0 — Shell

Visual chrome (top header + left sidebar) and placeholder routes for every Prio 1 destination. Anonymous everywhere — no user, no auth, no DB.

## What ships

- shadcn sidebar with three sections:
  - **Time tracking** — Time entry, Timesheets, Leave overview
  - **Admin** — Customers, Users, Contracts, Leave types (always visible; no role gating yet)
  - **Dev** — Animals
- Top header: brand "Timesheet Zone" left, theme toggle right. No user display.
- Placeholder routes resolving to `<ComingSoon slice="Sx" />`: time-entry (S7), timesheets (S10), leave-overview (S11), admin/{index,customers} (S1), admin/users (S2), admin/contracts (S3), admin/leave-types (S4).
- Landing card on `/`.

## What's deferred

- Auth, identity, login, `useCurrentUser`, `/admin/*` gating → S6 (OAuth via Entra ID).
- DB, entities, CRUD → S1.
- Sidebar Financials / Invoicing / Dashboard / Follow-up sections, item badges, mobile tuning → out of Prio 1.
