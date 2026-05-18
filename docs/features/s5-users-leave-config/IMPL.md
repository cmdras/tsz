# IMPL: S5 — Users (leave config)

Plan: [PLAN.md](./PLAN.md)
Started: 2026-05-17
Chapter: 6/6

## Chapter 1: Tests — done

- **Tests:** 0 failing → 17 failing, 0 regressions (11 unit, 6 integration).
- **Deviation:** Added minimal stub types in production so tests compile — `UserLeaveAllowance` entity, `AllowanceMode` enum, `UserLeaveAllowanceRequest`/`Response`, `DuplicateUserLeaveAllowanceException`, `UserLeaveAllowanceConfiguration`, `UserResponse`, plus `DefaultMode` on `LeaveType` and `Leaves` on `UserRequest`. No service logic added — `UserService` and `LeaveTypeService` untouched.

## Chapter 2: Schema + EF — done

- **Tests:** 17 failing → 17 failing, 0 regressions.
- **Deviation:** Gate criterion (strictly more tests passing) N/A — in-memory tests are unaffected by migration files and column type conversions; the test count advances in Chapter 3 when service logic is added. Migration `defaultValue` changed from `""` to `"Unlimited"` so existing `LeaveType` rows remain deserializable after migration.
- **Manual:** `~/.dotnet/tools/dotnet-ef database update --context AppDbContext` from `packages/api/`.

## Chapter 3: Service — done

- **Tests:** 11 failing → 0 failing, 0 regressions (integration unchanged: 6 failing, 56 passing).
- **Deviation:** `UserEndpoints.MapGet("/{id:guid}")` return type updated from `Ok<User>` to `Ok<UserResponse>` — required for compilation when `GetByIdAsync` changed return type; full endpoint wiring deferred to Chapter 4. `LeaveTypeSeeder` called before `UserSeeder` in `Program.cs` so leave types exist when user allowances are cross-seeded.

## Chapter 4: Endpoints — done

- **Tests:** 6 failing → 0 failing, 0 regressions (56 integration passing → 62 passing, 96 unit passing).
- **Deviation:** Only change needed was catching `DuplicateUserLeaveAllowanceException` in the PUT handler — all other endpoint wiring was already correct from Chapter 3's compilation fix.

## Chapter 5: FE data access — done

- **Gate:** typecheck clean; lint/format clean.
- **Deviation:** `allowanceModes` / `allowanceModeLabels` / `allowanceModeSchema` live in `leave-types.schemas.ts` (not `users.schemas.ts` as PLAN.md suggests) — same enum is used by both the LeaveType form (S4 follow-up) and the user leave-allowance rows, so consolidating avoids a duplicate enum. `leaves` on `userSchema` is `z.array(...)` without `.default([])` — TanStack Form's StandardSchema invariant rejects the input-vs-output mismatch that `.default()` produces; the form always supplies `leaves: []`. Touched `api/client.ts` to add `body?: string` on `ApiRequestError` so `updateUserFn` can distinguish the email-conflict 409 from the leave-allowance 409 by body text. Touched `routes/admin/users/-components/form.tsx` and `routes/admin/leave-types/-components/form.tsx` minimally — added `leaves: initial.leaves ?? []` and `defaultMode: initial.defaultMode ?? 'Limited'` to defaultValues so `satisfies` still typechecks; full UI lands in Chapter 6.
- **Manual:** none.

## Chapter 6: FE UI — done

- **Gate:** typecheck clean; format/lint clean; web tests 10 passed; api unit 96 passed; integration 62 passed (full suites, zero regressions).
- **Deviation:** Leaves card only renders on `$id` route (gated by optional `leaveTypes` prop) — `new.tsx` doesn't pass `leaveTypes` because the server unconditionally auto-populates on create per PLAN, so a picker on `/new` would be misleading. `leave-types.functions.ts` already exposed `listLeaveTypesForPickerFn`; route loader calls it in parallel with `fetchUserById`. Mode select switches to `Unlimited` → `totalDays` is force-reset to 0 (avoids stale value on toggle); input is `disabled` when mode is `Unlimited`. Picker is a controlled shadcn Select pinned to a sentinel value (`__pick__`) so the trigger always shows the "Add leave…" placeholder. Defaut LeaveType form gained a `defaultMode` Select rendered above `defaultDays`.
- **Manual:** none.

## Acceptance check

- ✓ `/admin/users/$id` renders a Leaves Card with inline rows (Name, Mode, TotalDays, Taken, Balance, ×) + Add-leave picker; PUT bundles all leave edits atomically.
- ✓ TotalDays disabled when Mode=Unlimited; `step="0.1"` + zod refine enforce 0–365 with ≤1 decimal.
- ✓ Create auto-populates one row per non-archived LeaveType for current year — verified by `UserServiceTests.CreateAsync_PopulatesLeavesForAllLeaveTypes`.
- ✓ Fresh-DB seed cross-product — verified by integration test + manual seeder review (`UserSeeder` runs after `LeaveTypeSeeder`).
- ✓ LeaveType form has Mode select; seeder writes Sickness=Unlimited, others=Limited; single new `AddLeaveTypeDefaultMode` migration.
- ✓ Add-leave picker filters out already-used types via `availableLeaveTypes`; picked row pre-fills from `DefaultMode`/`DefaultDays`.
- ✓ Removal + save hard-deletes the row — service replace-all semantics + integration test.
- ✓ Unique `(UserId, LeaveTypeId, Year)` enforced; duplicate insert throws `DuplicateUserLeaveAllowanceException` → 409 → form surfaces "Duplicate leave allowance" Sonner toast.
- ✓ New LeaveType created post-seed doesn't mutate existing users; appears in their picker once they load `$id`.
- ✓ No `[Authorize]` introduced; route remains under `/admin/` for S6 to gate.
