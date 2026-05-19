# Drop Counters table

## What

Resolve race condition on `Customer.Number` / `Contract.Number` assignment by removing the shared `Counters` infrastructure entirely. Each repository computes the next number inline as `MAX(Number) + 1` inside a `Serializable` transaction; SQLite's `BEGIN IMMEDIATE` serializes writers, so concurrent creates can't duplicate. Unique indexes on `Number` stay as belt-and-braces.

Side effect: numbers now start at `1` instead of `100000`. UI pads to 6 digits (`000001`) when displaying.

## Why

- Closes Github task *Counters: race condition on Number assignment*.
- Removes the `Common/Counters` cross-module coupling Evert flagged in the 13/05 review.
- Keeps each module self-contained — Customer and Contract own their own numbering, no shared service.
- Stays on the InMemory test provider (transaction is silently ignored; existing tests stay green).

## Scope

### Backend

1. Delete `packages/api/Common/Counters/` entirely (`Counter.cs`, `CounterConfiguration.cs`, `CounterKeys.cs`, `CounterSeeder.cs`, `CounterService.cs`, `ICounterService.cs`, `CountersModule.cs`).
2. Remove `Counters` DbSet from `AppDbContext`; drop `AddCountersModule` from `Program.cs`.
3. Migration: drop the `Counters` table.
4. `CustomerRepository.CreateAsync`: open `Serializable` transaction → compute `MAX(Number) + 1` (or `1` if empty) → add + `SaveChanges` → commit.
5. `ContractService.CreateAsync`: replace `_counterService.NextAsync(CounterKeys.Contract, …)` with the same inline pattern.
6. Keep `HasIndex(x => x.Number).IsUnique()` on Customer and Contract configurations.

### Frontend

7. Pad number display to 6 digits wherever `customer.number` / `contract.number` is rendered (list rows, detail headers). Single helper `formatEntityNumber(n)` in `packages/web/src/lib/` or inline `String(n).padStart(6, '0')` — TBD in PLAN.

### Tests

8. Drop `initialCounterValue` parameter from `CustomerRepositoryTests.CreateRepository` and `ContractServiceTests` fixture; stop seeding `Counters`.
9. Update assertions: `Create_FirstCustomer_GetsNumber100000` → `Create_FirstCustomer_GetsNumber1`; `Create_AssignsSequentialNumbers` switches from `100042 → 100043` to e.g. `42 → 43`.
10. Replace `Create_IncrementsCounterAndPersistsCustomerTogether` with a test that the inserted customer's `Number` is the previous `MAX + 1`.
11. Confirm integration tests (`CustomerEndpointsTests`, `ContractEndpointsTests`) no longer touch `Counters`.

## Not in scope

- Migrating the test suite off the InMemory provider.
- Extracting a `ContractRepository` to mirror `CustomerRepository` (separate refactor — `ContractService` keeps the inline tx for now).
- Switching `Number` to a SQLite-managed sequence (would require making `Number` the PK or adding a trigger; bigger change).
- Cross-instance safety — single-instance API assumption stands.

## Open / plan-time questions

- Where does the 6-digit padding helper live, and is it applied at component or schema/transform level?
- Should the migration include a data-preservation step for any existing `Customer` / `Contract` rows (dev DB only — wipe-and-reseed acceptable)?
