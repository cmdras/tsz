# PLAN: S4 — Leave Types admin catalog

Source: feature.md

## Goal

Stand up the admin catalog of leave-type definitions so S5 can layer per-user `(UserId, LeaveTypeId, Year, …)` on top and S7/S9/S11 can consume it. Replaces the `routes/admin/leave-types.tsx` S0 placeholder with the full CRUD + archive flow that mirrors S1 Customers.

## Approach

Add `Modules/LeaveTypes/` on the api side following the S1 layout: `LeaveType.cs`, `LeaveTypeConfiguration.cs`, `LeaveTypeService.cs`, `LeaveTypeEndpoints.cs`, `LeaveTypeContracts.cs`, `LeaveTypeSeeder.cs`. Register `DbSet<LeaveType>` in the existing `AppDbContext`. Entity: `Id` (Guid PK), `Name` (string, required, ≤100), `DefaultDays` (`decimal(5,1)`, 0–365), `IsArchived` (bool). No `Number`/`ICounterService` — tiny fixed catalog.

Case-insensitive uniqueness on `Name` is enforced two ways: EF config declares `HasIndex(x => x.Name).IsUnique()` with `Sqlite:Collation = NOCASE`, and `CreateAsync`/`UpdateAsync` pre-check existing names via `EF.Functions.Like` / `ToLower` comparison to return a friendly conflict response before EF throws. Endpoints copy Customers verbatim — paged GET (search, sort, showArchived), GET-by-id, POST, PUT, PATCH `/archive`, PATCH `/unarchive` — and do **not** add `[Authorize]`; S6 retroactively applies the admin policy across S1–S5. Seeder follows the existing static `SeedAsync(AppDbContext)` convention and inserts Holiday/ADV/Sickness/Ancienniteit/Holiday replacement at 20/5/0/0/0, all non-archived.

On the web side, delete the `routes/admin/leave-types.tsx` placeholder and replace it with the folder layout: `routes/admin/leave-types/{index,$id,new}.tsx` plus `-components/form.tsx`. Add `features/leave-types/leave-types.{server,schemas,functions}.ts` mirroring `features/customers/`. List page has Name search, "Show archived" toggle, sortable Name + DefaultDays headers, and the same pagination shell as Customers (UI hidden when total ≤ pageSize). Form is Card-wrapped, TanStack Form + Zod, Sonner toasts on success/error. Archive uses the same AlertDialog pattern; unarchive is a direct action. `DefaultDays` input accepts one decimal place; client schema rejects negatives, values >365, and more than one fractional digit.

## Acceptance criteria

- `/admin/leave-types` lists, creates, edits, archives, and unarchives leave types end-to-end.
- Duplicate `Name` (case-insensitive) is rejected by the service before EF throws, with a friendly conflict response surfaced as a Sonner error toast.
- `DefaultDays` accepts half-day values (e.g. `7.5`); rejects negatives, values >365, and >1 fractional digit.
- "Show archived" toggle includes archived rows; archived rows persist and remain referenceable by later S5/S7/S9 FKs.
- Name and DefaultDays column headers are sortable; search filters on Name only.
- A fresh database seeds Holiday (20), ADV (5), Sickness (0), Ancienniteit (0), Holiday replacement (0), all non-archived.
- No `[Authorize]` is added in this slice; the route lives under `/admin/` for S6 to gate.
