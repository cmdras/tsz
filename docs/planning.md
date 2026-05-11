# TSZ — Slice Plan

Spec source: `../poc-tsz/docs/product/{requirements.md,architecture.md}`.
Scope target: **Prio 1 only.** Prio 2 and Out-of-Scope items from the spec stay out.

## Locked conventions

- **Auth:** stub-first. In S0, 3 hardcoded users in code (Admin / User / Client Manager) + middleware that reads `X-Impersonate-User` header or `tsz-impersonate-user` cookie and attaches a `ClaimsPrincipal`. `useCurrentUser()` hook + header role-switcher dropdown on the web side. S2 moves users into the DB. Azure Entra ID swap happens after Prio 1 features work.
- **DB:** SQLite via EF Core. **Single `AppDbContext`** in `packages/api/Common/Database/`; every module registers its `IEntityTypeConfiguration<T>` into it. New DB file `tsz.db`. The legacy `animals.db` + `AnimalDbContext` stays separate until S11 deletes it.
- **IDs:** GUID primary keys. Customers additionally get a 6-digit business `Number` (sequential, starts at 100000, generated via shared `Common/Counters/` service — `ICounterService.NextAsync("customer")`). Contracts' identifier strategy is TBD in the S3 plan (the original `K-0001` convention is dropped).
- **Deletes:** soft-delete (`is_archived` flag). API exposes PATCH `/{id}/archive` + PATCH `/{id}/unarchive`; no hard DELETE on admin entities. Lists show active by default + a "Show archived" toggle. Pickers (S3+) hide archived rows. No cascade.
- **Routes:** admin pages nested under `/admin/*`. Edit UX = dedicated route per entity (`/admin/<entity>/$id` and `/admin/<entity>/new`). End-user pages stay at top level.
- **Role gating:** `/admin/*` is Admin-only. UI loader redirect for non-admins established in S0; server-side gating on the API route group established in S1 with the first admin endpoint.
- **UI language:** English by default; occasional Dutch terms (e.g. month names, "ancientiteit") to be confirmed with the PO.
- **Non-editable days in v1:** weekends only. Belgian holidays land in Prio 2 via openholidaysapi.
- **Approval:** self-submit. Clicking "submit" sets the week to `approved`; no separate approver step.
- **UI library:** shadcn/ui only. Forms wrapped in shadcn `Card`. Mutations toast via shadcn `Sonner`. Archive confirms via shadcn `AlertDialog`; unarchive is direct.
- **Forms:** TanStack Form + Zod (via the form's validator adapter, never inline). `FieldError` component per field. Country fields use a shadcn `Select` with Belgium default + "Other" → free-text fallback.
- **Server functions:** every `createServerFn` uses `.inputValidator(<zodSchema>)`; schemas live with the server function and are imported by the route's form.
- **Reference scaffold:** the `animals` module (API: `packages/api/Modules/Animals/*`, web: `packages/web/src/api/animals/*` + `packages/web/src/routes/animals/*`) is the template every slice copies — though admin slices wrap forms in `Card` (animals is left as-is until S11 deletes it). Deleted in S11.

## Slicing principles

- **Vertical** — each slice ships DB → API → UI so it's demonstrable on its own.
- **Forward-only dependencies** — no slice references something that "will exist next slice."
- **Pattern-establishing early** — S1 sets the conventions every later slice copies.
- **Right-sized for piv** — one slice = one `/piv-plan` → `/piv-implement` → `/piv-validate` cycle.

## Dependency picture

Time Entry sits at the top. It needs a current user (auth), tasks (via Contracts, which need Customers + Users), leaves (via Leave Types + per-user leave config), and date logic. So admin CRUD first, user features on top.

## Slices

### S0 — Shell + Stub Auth

**Includes:** stub auth (3 hardcoded users + middleware + `/api/auth/me`), `useCurrentUser` hook, RoleSwitcher header dropdown, top header + left sidebar chrome (shadcn `sidebar`), placeholder routes for every Prio 1 destination (Time entry, Timesheets, Leave overview, plus admin entity placeholders), `/admin/*` UI loader redirect for non-Admin users.

**Why first:** lands the visual shell and the identity that powers it together, so every later slice ships features into a working app instead of bolting nav on alongside them. Makes manual verification feel real from S1 onward.

**No DB / no entity:** `AppDbContext`, `tsz.db`, and the Counters service all wait for S1 to introduce them alongside the first persisted entity.

### S1 — Foundation + Customers

**Includes:** persistence foundation (`AppDbContext`, EF Core migrations, Counters service), Customer entity (number, name, address fields, contact name/email, `is_archived`), CRUD endpoints with server-side admin role gating, list page, create/edit form, archive/unarchive, seed data.

**Why bundled:** the persistence foundation needs a feature on top to be testable end-to-end. Bundling Customers lands the OpenAPI codegen pipeline, the first server-side admin gating, and the Zod/form/server-function pattern in one slice. The resulting structure becomes the template every later slice copies — so it's worth getting right here.

**Risk:** largest data-side slice (sets DB + first CRUD pattern), but lighter than it was before S0 absorbed the chrome and auth boundary.

### S2 — Users (basic CRUD)

**Includes:** name, email, role (Admin / User / Client Manager). No per-user leave config yet.

**Why next:** Contracts (S3) needs a consultant reference, so Users has to land before Contracts. Splitting the basic CRUD from the leave-config sub-form (deferred to S5) keeps this slice small and aligned with the S1 pattern.

### S3 — Contracts + Tasks

**Includes:** Contract entity linked to a Customer and a User (consultant); subject, start/end dates; **one Contract has many Tasks (1:N)**, each task with name + day rate. The Task (not the Contract) is what gets picked as a row in Time Entry later. Customer + Consultant pickers in the form pull from S1/S2.

**Why now:** last admin piece blocking Time Entry's "pick a task" UX. Reuses both prior slices.

### S4 — Leave Types

**Includes:** flat CRUD for leave types (holiday, sick, ancientiteit, ADV, etc.).

**Why here:** independent of S1–S3, so it could have gone earlier. Slotted mid-chain instead — the harder admin shapes (Users, Contracts) are done while you're fresh, and Leave Types becomes a warm-down before the more complex per-user leave config in S5.

### S5 — Users (leave config)

**Includes:** per-user leave-type allowance — Allowed / NotAllowed / Limited(N days), per-year totals / taken / balance. Multi-row sub-form on the user edit page. Default leave allotments on user creation (per spec: leave 20, ADV 5, ancientiteit 0, sickness 0).

**Why split from S2:** this is a sub-form with state semantics, not a flat CRUD — bundling it into S2 would make S2 the biggest slice in the plan before patterns are established.

**Why before Time Entry:** the leave picker in Time Entry is gated by what each user is allowed to take.

### S6 — Time Entry (single-week grid)

**Includes:** per-week view locked to the current week; add-task picker (filtered to in-range contracts + consultant); rows of task × day cells with time entry; total per task, per day, per week; weekends non-editable; manual save (button).

**Why split from S7:** Time Entry is the biggest feature in the spec. Landing the grid mechanics on a single week first lets us verify the load-bearing UX before introducing navigation state and the data-loss surface of auto-save.

### S7 — Time Entry (navigation + auto-save)

**Includes:** week navigation (prev/next/today/calendar picker); auto-save on navigation away or week change — replaces the manual save from S6.

**Why split from S6:** auto-save pairs naturally with navigation because the triggers *are* navigation events. Splitting them out lets us prove the grid behavior in isolation first, then layer state transitions on top.

### S8 — Time Entry (leaves + hotkeys + submit)

**Includes:** add-leave picker (filtered by per-user allowed leave types); hotkeys (`d` = full day, `h` = half day, `del` = empty); submit-for-approval action that marks the week `approved`.

**Why split from S6/S7:** additive features on the same grid; cheap to layer in once the grid and navigation work.

### S9 — Timesheets

**Includes:** per-month read view of entered time entries; light-green → dark-green coloring based on weekly approval status; per-task day-count summary; month navigation (prev/next/today).

**Why before Leave Overview:** same calendar-grid shape we'll reuse, and the data already exists from S6–S8.

### S10 — Leave Overview

**Includes:** per-year calendar with leave days marked; current-date and weekend indicators; balance summary per leave type for the year; year navigation.

**Why last feature:** depends on everything else — entries to display, leave types defined, per-user balances populated.

### S11 — Cleanup

**Includes:** delete the `animals` module (API + web + tests + DB file), remove route entry, clean up references. Prepare the auth boundary for Entra-ID integration (Entra swap itself may be its own slice if scheduled before v1 ships).

**Why last:** animals stays as the live reference scaffold while we copy from it; only safe to delete once every slice has its own concrete example to refer back to.

## Workflow per slice

Each slice runs the full PIV cycle, fired manually:

1. Draft a short `docs/features/<slice>/feature.md` describing what to build.
2. User invokes `/piv-plan <feature.md>` — produces a `PLAN.md` in a slot folder via a Socratic conversation.
3. User invokes `/piv-implement <PLAN.md>` — executes the plan strictly in scope, writes `IMPL.md`.
4. User invokes `/piv-validate` from the slot folder — produces `VALIDATION.md` with multi-reviewer verdict.

No stage auto-chains into the next — the user inspects each artifact and decides when to advance.

## Open items to resolve in-slice

- **Contract-number generation strategy (S3):** decide in S3's plan. Customers use GUID + Counters service; contracts may follow the same shape or use something different per PO direction.
- **Time-entry storage format (15-min integer minutes vs float hours vs other):** decide in S6's plan.
- **Whether Entra swap is a pre-v1 slice or a post-Prio-1 follow-up:** revisit after S10.

## Resolved in earlier slices

- **S1:** Edit UX = dedicated `/admin/<entity>/$id` route. `addressCountry` = shadcn Select (Belgium default + "Other" → free text). Customer-number = GUID PK + 6-digit `Number` via shared `Common/Counters/` service (starts at 100000, seeded rows consume numbers).
