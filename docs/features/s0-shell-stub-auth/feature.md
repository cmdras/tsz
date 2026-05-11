# S0 — Shell + Stub Auth

Pre-S1 slice. Establishes the visual chrome (top header + left sidebar) and the stub-auth boundary that powers it, so every later slice lands features into a working shell. No domain entity, no CRUD, no DB writes from feature code — the only persistence concern is a `Counter` table-free `AppDbContext` scaffold (deferred to S1).

Spec source: `../poc-tsz/docs/product/requirements.md` (chrome look-and-feel from screenshots `user-time-entries.png`, `admin-settings.png`, `user-leave-overview.png`).

## Scope

### Stub auth (moved here from S1)

Goal: swappable auth boundary so Entra ID can drop in post–Prio 1 without touching feature code.

- 3 hardcoded users in `packages/api/Common/Auth/StubUsers.cs`. Roles: Admin, User, Client Manager. Plausible Belgian names, one per role.
- ASP.NET middleware (or auth handler) reads `X-Impersonate-User` header **or** `tsz-impersonate-user` cookie, maps to a stub user, attaches a `ClaimsPrincipal`. Default fallback = the Admin user.
- `GET /api/auth/me` returns `{ id, name, email, role }` for the current principal.
- No admin-role gating on an entity yet (none exists). The pattern — `[Authorize(Roles="Admin")]` on the route group, or equivalent endpoint filter — is decided here but applied in S1 when the first admin endpoint lands.

### Web chrome

Replace the current `__root.tsx` toy nav with a real layout matching the spec aesthetic.

- **Top header**: brand "Timesheet Zone" left; right side = current-user display + theme toggle. User display is a shadcn `DropdownMenu` (avatar + name) — opening it reveals the **RoleSwitcher** (3 stub users with role labels). Clicking a user sets the `tsz-impersonate-user` cookie and triggers a router invalidate so loaders re-fetch.
- **Left sidebar** (shadcn `sidebar` component, installed via `bunx shadcn@latest add sidebar`):
  - User card at top (avatar + name + role) — mirrors the spec.
  - Section: **Time tracking** → Time entry, Timesheets, Leave overview.
  - Section: **Admin** (Admin-only, hidden for User/Client Manager) → Customers, Users, Contracts, Leave types.
  - Section: **Dev** (always visible, until S11) → Animals.
  - No Financials / Invoicing / Follow up / Dashboard sections — those are out of spec scope.
- Same chrome for both end-user and `/admin/*` routes. No second layout.
- `useCurrentUser()` hook returning `{ id, name, email, role }`. Data source — `__root.tsx` route loader hitting `/api/auth/me` vs React context populated at startup — `/piv-plan` decides.
- `/admin/*` route loader redirects non-Admin users to `/`. Server-side gating waits for S1's first admin endpoint.

### Placeholder routes

Every Prio 1 destination linked in the sidebar must resolve to *something*, so navigation feels real during manual verification.

- `routes/index.tsx` — currently the toy "home"; rewrite to a simple landing card ("Welcome, <name>") with the user's role.
- `routes/time-entry.tsx` — placeholder ("Coming in S6").
- `routes/timesheets.tsx` — placeholder ("Coming in S9").
- `routes/leave-overview.tsx` — placeholder ("Coming in S10").
- `routes/admin/index.tsx` — placeholder admin landing ("Coming in S1").
- `routes/admin/customers.tsx`, `users.tsx`, `contracts.tsx`, `leave-types.tsx` — each a "Coming in S<n>" placeholder.
- Placeholder pages share a single `<ComingSoon slice="S6" />` component (shadcn `Card`, slice label, one-line description). Admin placeholders sit behind the `/admin/*` redirect guard.

### Animals

Stays in the sidebar under the **Dev** section. It's still the reference scaffold S1–S10 copy from; S11 deletes it along with the section.

### Tests (Vitest only)

- `useCurrentUser` returns the impersonated user when the cookie is set; falls back to Admin otherwise.
- `RoleSwitcher` writes the cookie + invalidates the router on click.
- Sidebar: admin section is hidden for non-Admin roles.
- `/admin/*` loader redirects non-Admin to `/`.
- No xUnit / API integration tests yet — deferred until S1.

## Out of scope

- `AppDbContext`, `tsz.db`, EF migrations, Counters service — all land in S1 with the first entity.
- Any CRUD, list/edit UI, or admin entity work.
- Sidebar Financials / Invoicing / Dashboard / Follow up sections (visible in spec screenshots, not in Prio 1).
- Sidebar item badges (counts next to "To do list", "Approve", etc. in the spec — not Prio 1).
- Real Entra ID auth (post–Prio 1).
- Mobile / collapsed-sidebar tuning beyond shadcn's defaults.

## Conventions established here (copied by S1–S10)

1. Stub-auth `useCurrentUser()` hook + `RoleSwitcher` in header dropdown (until the Entra swap).
2. Single chrome (top header + left sidebar) for both end-user and admin routes.
3. `/admin/*` admin-only via UI loader redirect + (from S1) server-side role gating on the API route group.
4. New top-level features get a sidebar entry under either **Time tracking** or **Admin**.
5. `<ComingSoon slice="Sx" />` placeholder pattern for routes whose backing slice hasn't landed.

## Open / plan-time questions for `/piv-plan` to surface

- `useCurrentUser` data source — `__root.tsx` route loader hitting `/api/auth/me`, or React context populated at startup?
- Cookie vs header for the impersonation channel. Cookie auto-sends; header needs openapi-fetch middleware. Probably cookie only in S0; revisit if tests need per-request override.
- Visual style of the header — match spec's red bar exactly, or use a theme-friendly variant via shadcn tokens? Spec uses red; current app uses default shadcn theme.
- Sidebar collapse behavior — shadcn's default (collapse to icon-only rail) is fine, or always-expanded for v1?
- Where the existing `Home` and `Animals` links go. Proposal: drop `Home` from the nav (the brand is the home link); `Animals` moves under **Dev**.
- `/admin/index.tsx` — empty landing card, or redirect to the first admin entity once S1 lands? S0 ships the landing; S1 may revisit.
