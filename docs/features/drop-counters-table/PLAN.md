# PLAN: Drop Counters table, inline MAX+1 per module

Source: feature.md

## Goal

Remove the shared `Common/Counters` infrastructure. `CustomerRepository.CreateAsync` and `ContractService.CreateAsync` each compute `MAX(Number) + 1` inline inside a `Serializable` transaction (`BEGIN IMMEDIATE` on SQLite). Numbers restart at `1`; existing UI padding (`padStart(6, '0')`) is unchanged. Single-instance API assumption stands.

## Approach

**Backend deletions.** Delete `packages/api/Common/Counters/` (seven files: `Counter.cs`, `CounterKeys.cs`, `CounterConfiguration.cs`, `CounterSeeder.cs`, `CounterService.cs`, `ICounterService.cs`, `CountersModule.cs`). Remove the `Counters` DbSet and `CounterConfiguration` application from `AppDbContext.cs`. Remove `AddCounters()` from `Program.cs` and the `CountersModule.SeedAsync(...)` call from `Common/Database/DatabaseModule.cs`.

**Migration.** Run `dotnet ef migrations add RemoveCounters` against the `AppDb` context. Do not hand-edit the generated migration or the snapshot. No data-preservation step — wipe-and-reseed for dev DBs is acceptable.

**CustomerRepository.CreateAsync.** Open a `Serializable` transaction via `_dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct)`. Compute `next = (await _dbContext.Customers.MaxAsync(c => (int?)c.Number, ct) ?? 0) + 1`. Set `customer.Number = next`, `Add`, `SaveChangesAsync`, `CommitAsync`. The `HasIndex(x => x.Number).IsUnique()` configuration stays as belt-and-braces.

**ContractService.CreateAsync.** Drop the `ICounterService` constructor dependency. Keep `ValidateRequestAsync` outside the tx. Then open Serializable tx, compute next number the same way against `_dbContext.Contracts`, build the contract + tasks, `Add`, `SaveChangesAsync`, `CommitAsync`. `LoadReferencesAsync` runs after commit (unchanged position). Unique index stays.

**Frontend.** No changes. The five existing inline `String(n).padStart(6, '0')` call sites stay as-is.

**Tests.**
- `api.tests/Customers/CustomerRepositoryTests.cs`: drop `initialCounterValue` parameter from the `CreateRepository` helper; remove the `Counters.Add(...)` seeding line. Rewrite assertions so first customer expects `Number == 1` and the sequential test uses small numbers (e.g. `42 → 43`). Replace `Create_IncrementsCounterAndPersistsCustomerTogether` with a test that asserts the inserted customer's `Number` equals the previous `MAX + 1`.
- `api.tests/Customers/CustomerServiceTests.cs`: same fixture cleanup (drop `initialCounterValue`, remove `Counter` seeding); adjust any number-specific assertions to match the new baseline.
- `api.tests/Contracts/ContractServiceTests.cs`: remove the `Counter` seed and the `CounterService` instantiation; constructor injection drops `ICounterService`. Adjust number assertions.
- `api.tests.integration/Customers/CustomerEndpointsTests.cs` and `api.tests.integration/Contracts/ContractEndpointsTests.cs`: remove the `Counter` clear/seed lines from `InitializeAsync`.

## Acceptance criteria

- No file under `packages/api/` (including tests and migrations snapshot) references `Counter`, `CounterService`, `ICounterService`, `CounterKeys`, `CountersModule`, or `AddCounters`.
- `dotnet build` succeeds; `dotnet test` is green for `api.tests` and `api.tests.integration`.
- A new EF migration drops the `Counters` table; `AppDbContextModelSnapshot.cs` no longer contains a `Counter` entity.
- On a fresh DB, the first Customer and first Contract created each get `Number == 1`; subsequent rows get the previous `MAX + 1`.
- Frontend rendering of customer/contract numbers is unchanged (still 6-digit zero-padded via inline `padStart`).
