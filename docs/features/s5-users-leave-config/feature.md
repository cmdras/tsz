# S5 — Users (leave config)

- Per-user leave allowance: row keyed `(UserId, LeaveTypeId, Year)` with `Mode` (Allowed / NotAllowed / Limited), `TotalDays` (decimal, only meaningful when Limited), and derived `Taken` / `Balance` (Taken=0 until S7 lands; Balance = TotalDays − Taken for Limited, blank otherwise).
- New `Modules/UserLeaveAllowances/` on api side (entity + configuration + service + endpoints + contracts), registered into existing `AppDbContext`. No new top-level route — sub-resource of User.
- Sub-form on `/admin/users/$id` (and `/new`) listing the user's leave rows for a selected year. "Add leave" picker filtered to non-archived LeaveTypes not already on the user for that year. Edit row = modal/inline form (Name read-only, Mode select, TotalDays only when Limited, Year).
- On user create: auto-populate rows for the current year from every non-archived LeaveType, using `LeaveType.DefaultDays` as `TotalDays`. Default Mode TBD (plan-time question).
- Archived users keep their rows; archived LeaveType rows already on a user stay visible (read-only), but the LeaveType is hidden from the "Add leave" picker — same convention as Customer/User in Contracts.
- No `[Authorize]` added in this slice — S6 retroactively gates `/admin/*`.

## Open / plan-time questions

- **Terminology:** spec uses `Allowed / NotAllowed / Limited`; the PoC UI mock shows `Unlimited / Limited` (no NotAllowed visible). Pick the canonical set for both DB enum and UI labels.
- **Default Mode on auto-population:** LeaveType currently has only `DefaultDays`. Options: (a) add `DefaultMode` column to LeaveType (S4 migration follow-up), (b) infer from `DefaultDays` (0 → Unlimited/NotAllowed, >0 → Limited), (c) always default to Limited and let admin flip.
- **Year scoping on the user-edit page:** year picker (multi-year history visible) vs always-current-year-only for v1. Spec shows `Year` as an editable field per row, implying multi-year.
- **Sub-form UX:** modal "Edit Leave" (matches PoC screenshots) vs inline editable row. S3 Contracts used inline tasks — consistency vs spec fidelity.
- **Taken computation:** stored column updated by S7 vs computed-on-read from time entries. S5 ships with Taken=0 either way; pick the shape now so S7 doesn't need a migration.
- **Backfill for existing seeded users:** S2's seeded users predate this entity. Seeder/migration step that creates current-year rows for them, or accept they start empty until edited.
- **Adding a LeaveType later:** does it auto-append to existing users' current-year config, or only show up via "Add leave"? (Leaning manual via "Add leave" — keeps server-side magic minimal.)
- **Validation when Taken > new TotalDays:** allow with warning, or reject. Relevant from S7 onward but the rule belongs in the service now.
