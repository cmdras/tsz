# PLAN: S5 — Users (leave config)

Source: feature.md

## Goal
Per-user leave-allowance sub-form on `/admin/users/$id` (and `/new`), keyed `(UserId, LeaveTypeId, Year)` with Mode (Unlimited/Limited) + TotalDays. Auto-populated on user create from `LeaveType.DefaultMode` + `DefaultDays`; current-year only. Lays the data needed by S7 (leave picker + balance) and S11 (overview), with no `[Authorize]` (S6 gates).

## Approach
S4 follow-up first: add `DefaultMode` enum column (`Unlimited` | `Limited`) to `LeaveType` via a new EF migration. `LeaveTypeSeeder` updated so Sickness = Unlimited and the other four = Limited. `LeaveType` admin form + `leave-types.schemas.ts` gain a Mode select; the existing list page picks up the column read-only (no new sort).

New `Modules/UserLeaveAllowances/` on the API mirroring the S1 layout: `UserLeaveAllowance.cs` (Id Guid PK, UserId FK, LeaveTypeId FK, Year int, Mode enum, TotalDays `decimal(5,1)`), `UserLeaveAllowanceConfiguration.cs` declaring the FKs (no cascade) + unique index on `(UserId, LeaveTypeId, Year)`. Registered as a `DbSet` in `AppDbContext`. No service of its own — owned by `UserService`. No standalone endpoints.

`UserService.GetByIdAsync` switches to eager-load `User.Leaves.Where(year == currentYear)` and projects into a `UserResponse` that carries `Leaves[]` (each row: `Id`, `LeaveTypeId`, `Name` from joined LeaveType, `Mode`, `Year`, `TotalDays`, `Taken` (always 0 in S5), `Balance` (= TotalDays − Taken when Limited, else null)). `Taken`/`Balance` are DTO-only — no stored columns; S7 lights them up by summing time entries at read time. `CreateAsync` auto-populates one row per non-archived LeaveType for the current year using `DefaultMode` + `DefaultDays`. `UpdateAsync` (and the request) gains a `Leaves[]` array and runs replace-all semantics: match by `Id` → update Mode/TotalDays; missing Ids → hard-delete; new entries (Id null/empty) → insert. Inserts validated against the unique key for friendly conflict response. `UserSeeder` extended: after both Users and LeaveTypes are seeded, write one current-year row per (existing User × non-archived LeaveType) using each LeaveType's `DefaultMode`/`DefaultDays`.

Web: extend `routes/admin/users/-components/form.tsx` with a second `Card` "Leaves" placed below the existing General card. Inline editable table via TanStack Form array field — same shape as S3 Contracts→Tasks: columns Name (read-only label), Mode (shadcn Select), TotalDays (number input, `disabled` when Mode=Unlimited, accepts one decimal), Taken (read-only), Balance (read-only), Remove button. Below the table, an "Add leave" control: a shadcn Select listing non-archived LeaveTypes not already in the array (sourced from a new `listLeaveTypesForPickerFn` server fn or piggybacked on the existing list call with `pageSize=large`); picking one appends a row pre-filled from the LeaveType's `DefaultMode`/`DefaultDays`. `features/users/users.schemas.ts` gains `userLeaveAllowanceSchema` + `userLeaveAllowanceModeSchema`; the existing `userSchema` extends to include `leaves: z.array(userLeaveAllowanceSchema)`. `users.server.ts` + `users.functions.ts` updated against the regenerated `api/schema.ts` after `bun run gen:api`. `routes/admin/users/new.tsx` initial form value passes `leaves: []` (server fills on create); `$id.tsx` passes the loaded `leaves` through.

## Acceptance criteria
- `/admin/users/$id` renders a "Leaves" Card with inline rows (Name, Mode, TotalDays, Taken, Balance, Remove) and an "Add leave" picker; saving the form persists all leave changes atomically via the bundled PUT.
- TotalDays input is disabled when Mode=Unlimited; accepts one decimal place, 0–365; rejects negatives and >1 fractional digit.
- Creating a new user auto-populates one row per non-archived LeaveType for the current year, using each LeaveType's `DefaultMode` + `DefaultDays`.
- Fresh-DB seed: every seeded user × every non-archived LeaveType produces a current-year `UserLeaveAllowance` row.
- `LeaveType` admin form has a Mode select (Unlimited/Limited); seeder writes Sickness=Unlimited and Holiday/ADV/Ancienniteit/Holiday replacement=Limited; existing migration history extended with a single new `DefaultMode` migration.
- "Add leave" picker lists only non-archived LeaveTypes not already in the user's current-year array; after picking, the row appears pre-filled.
- Removing a row + saving hard-deletes the `UserLeaveAllowance` record (verified by re-loading the user).
- `(UserId, LeaveTypeId, Year)` is enforced unique; duplicate insert returns a friendly conflict surfaced as a Sonner error toast.
- Creating a new LeaveType after users exist does not mutate existing users; the new type appears in their "Add leave" picker.
- No `[Authorize]` added in this slice; the route stays under `/admin/` for S6 to gate.
