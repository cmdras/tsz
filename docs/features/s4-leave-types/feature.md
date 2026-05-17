# S4 — Leave Types

- Admin catalog of leave-type definitions. S5 layers per-user `(UserId, LeaveTypeId, Year, mode, totalDays, …)` on top; S7/S9/S11 consume it.
- Fields: `Id` (Guid, PK), `Name` (string, required, unique case-insensitive), `DefaultDays` (int ≥ 0, prefill used by S5 on new-user creation), `IsArchived`. No `Number`/counter (tiny fixed catalog).
- Replaces `routes/admin/leave-types.tsx` S0 placeholder with the standard folder layout (`index.tsx`, `$id.tsx`, `new.tsx`, `-components/`, `-schemas.ts`, `-server.ts`). New `Modules/LeaveTypes/` on api side, registered into existing `AppDbContext`.
- Copies S1 pattern verbatim: CRUD endpoints + PATCH `/archive` + `/unarchive`, Admin-gated route group, list page with search + Show archived toggle, Card-wrapped form, Sonner toasts, AlertDialog on archive only.
- Archived types stay referenced by any later S5/S7/S9 FKs (no cascade); just hidden from pickers — same convention as archived Customer/User in Contracts.
- Seed: `Holiday` (20), `ADV` (5), `Sickness` (0), `Ancienniteit` (0), `Holiday replacement` (0).

## Open / plan-time questions

- Unique-name enforcement — DB unique index on lowercased `Name` (computed column / generated column) vs. service-layer check inside `CreateAsync`/`UpdateAsync`. SQLite-friendly?
- `DefaultDays` storage — plain `int`, or do we want fractional support (half-days) now? S5/S7 will need to answer this anyway; cheaper to lock it in at S4.
- Search field on a ~5-row catalog — keep for pattern consistency (agreed), but confirm it should still filter on `Name` only (no other text fields exist).
