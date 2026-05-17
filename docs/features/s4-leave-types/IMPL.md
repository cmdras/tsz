# IMPL: S4 — Leave Types admin catalog

Source: [PLAN.md](./PLAN.md)
Started: 2026-05-17
Chapter: 6/6

## Chapter 1: Tests — done

- **Tests:** 0 failing → 37 failing, 0 regressions (22 unit + 15 integration, all new tests fail with `NotImplementedException`).
- **Deviation:** Created minimal stub types (`LeaveType`, `LeaveTypeRequest`, `LeaveTypeService` with `NotImplementedException` bodies, `LeaveTypeEndpoints` with throwing handlers, `AppDbContext.LeaveTypes` DbSet, DI + route registration in `Program.cs`) so tests compile and so the three "NonExistingId → 404" tests don't accidentally pass against missing routes. No business logic, no EF config, no migration — that's chapter 2.
- **Carry-forward:** `DuplicateLeaveTypeNameException` is defined alongside `LeaveTypeService` as the conflict signal; chapter 4 endpoint needs an exception filter or try/catch to map it to 409.

## Chapter 2: Schema + EF — done

- **Tests:** 37 failing → 37 failing, 0 regressions.
- **Deviation:** Used `UseCollation("NOCASE")` on the `Name` property (column-level collation) rather than an index annotation — EF Core 10 emits `collation: "NOCASE"` on the column in the migration, which SQLite propagates to any index on that column. First generated migration lacked the collation; removed and regenerated.
- **Manual:** Run `dotnet ef database update` (or let the app auto-migrate) to apply the `AddLeaveTypes` migration before first use.

## Chapter 3: Service — done

- **Tests:** 37 failing → 15 failing, 0 regressions (all 22 unit tests now pass; 15 integration tests still fail — endpoints not yet implemented).

## Chapter 4: Endpoints — done

- **Tests:** 15 failing → 0 failing, 0 regressions (all 54 integration tests pass).
- **Deviation:** `DuplicateLeaveTypeNameException` maps to `TypedResults.Problem(..., statusCode: 409)` using the same `ProblemHttpResult` pattern as Contracts' 422 — keeps the typed result union consistent with the rest of the codebase. `LeaveTypeSeeder` also wired up here (no dedicated tests, fits naturally with the endpoint chapter).
- **Carry-forward:** The `ToLower()` comparisons in service queries intentionally use LINQ-to-SQL translation; IDE suggests `StringComparison.OrdinalIgnoreCase` but that is not translatable by EF Core and would throw at runtime — leave as-is.

## Chapter 5: FE data access — done

- **Deviation:** `src/api/schema.ts` regenerated via `npx openapi-typescript http://localhost:5000/openapi/v1.json` (API runs on 5000 without `--no-launch-profile`; the `gen:api` script targets 5204). Ran `bun run check:fix` after to fix generated-file formatting.
- **Deviation:** `searchSchema.archived` maps to `showArchived` when calling `getLeaveTypes` — the API query param is `showArchived`, but `archived` is the conventional key in the search schema (matching contracts). Mapped in `fetchLeaveTypes` handler.

## Chapter 6: FE UI — done

- **Deviation:** `leaveTypeSchema` refine changed from `Number.isInteger(value * 10)` to `/^\d+(\.\d)?$/.test(String(value))` — floating-point multiplication (e.g. `7.1 * 10 = 71.000...0035`) causes `Number.isInteger` to incorrectly reject valid one-decimal values. String representation is stable for these inputs.
- **Deviation:** `createLeaveTypeFn`/`updateLeaveTypeFn` now catch 409 and re-throw with a descriptive message so the Sonner toast shows "A leave type with this name already exists." instead of a generic fallback — `ApiRequestError` only carries the status code, not the body.
- **Manual:** `bun run build` (in `packages/web`) regenerates the TanStack Router route tree; the `check` script alone does not trigger the plugin. Must run at least once after adding new route files.

## Acceptance check

- ✓ `/admin/leave-types` lists, creates, edits, archives, and unarchives leave types end-to-end — all routes implemented; 84 unit + 54 integration tests pass.
- ✓ Duplicate `Name` (case-insensitive) rejected before EF throws — service pre-check + NOCASE index; 409 → descriptive Sonner toast via catch in server function.
- ✓ `DefaultDays` accepts half-day values (7.5); rejects negatives, >365, >1 fractional digit — Zod schema with string-based refine; `step="0.1"` on input.
- ✓ "Show archived" toggle — `archived` search param, `showArchived` mapped to API; archived rows render at 50% opacity with Unarchive button.
- ✓ Name and DefaultDays headers sortable; search filters on Name only — `SortableHeader` on both columns, sort slug `defaultdays` maps to `DefaultDays`.
- ✓ Fresh database seeds Holiday (20), ADV (5), Sickness (0), Ancienniteit (0), Holiday replacement (0), all non-archived — `LeaveTypeSeeder` wired in `Program.cs`.
- ✓ No `[Authorize]` added — endpoints have no auth attribute; S6 retroactively applies admin policy.
