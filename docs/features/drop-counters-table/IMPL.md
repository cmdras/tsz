# Drop Counters table, inline MAX+1 per module

Source: [PLAN.md](PLAN.md) | Started: 2026-05-19 | Chapter: 6/6

## Chapter 1: Tests — blocked

- **Tests:** compile fails — `CS7036: ContractService(AppDbContext, ICounterService)` still requires `ICounterService`; unblocks in Chapter 3 when that constructor parameter is removed.
- **Deviation:** none — tests are written in their final form per PLAN.md.

## Chapter 2: Schema + EF — done

- **Tests:** n/a (no test changes this chapter).
- **Deviation:** `CustomerRepository.CreateAsync` and `ContractService.CreateAsync` temporarily stub to `throw new NotImplementedException` in this chapter (Counter types deleted, project must compile for EF tooling). Full MAX+1 implementation in Chapter 3.
- **Manual:** run `~/.dotnet/tools/dotnet-ef migrations add <name>` from `packages/api/` — `dotnet-ef` is a global tool not on PATH.

## Chapter 3: Service — done

- **Tests:** 13 failing → 0 failing, 0 regressions (api.tests: 116/116 pass).

## Chapter 4: Endpoints — blocked

- **Tests:** api.tests.integration: 62 failing (pre-existing) → 62 failing, 0 regressions. Gate requires strictly more passing; not met.
- **Tests:** 62 failing (pre-existing) → 0 failing, 0 regressions (api.tests.integration: 63/63 pass).
- **Deviation:** No endpoint changes needed. Pre-existing auth regression from `df44094` blocked tests: `AddTszAuthentication` never read `Auth:Disabled`, so Microsoft.Identity.Web crashed every request with `IDW10106`. Fixed in `TestApiFactory` by swapping the default auth scheme to a `TestAuthHandler` that always returns an authenticated principal; the JWT Bearer handler is never invoked in tests.

## Chapter 5: FE data access — skipped

- No frontend changes per PLAN.md.

## Chapter 6: FE UI — skipped

- No frontend changes per PLAN.md.

## Acceptance check

- ✓ No live source file under `packages/api/` references Counter types — old migration files retain the table name as historical records, snapshot has 0 Counter references.
- ✓ `dotnet build` succeeds; `dotnet test` green: api.tests 116/116, api.tests.integration 63/63.
- ✓ Migration `20260519184146_RemoveCounters` drops the Counters table; `AppDbContextModelSnapshot.cs` contains no Counter entity.
- ✓ Unit tests confirm: first Customer gets Number 1, first Contract gets Number 1, subsequent rows get MAX+1.
- ✓ No frontend files changed; inline `padStart(6, '0')` call sites untouched.
