# IMPL: S3 — Contracts + Tasks

Plan: docs/features/s3-contracts-tasks/PLAN.md
Started: 2026-05-15T08:30:00Z
Finished: 2026-05-15T10:00:00Z

## Acceptance check

- [✓] `ICounterService` exists in `Common/Counters/`; `CustomerService.CreateAsync` uses it; inline `MaxAsync + 1` removed.
- [✓] Counters table migration applied (`AddContractsAndCounters`); `CounterSeeder` initializes rows from high-water marks after entity seeders run.
- [✓] `GET /api/contracts` paginates, searches Subject/Customer/Consultant, sorts all six columns, hides archived unless `archived=true`.
- [✓] `POST /api/contracts` assigns Number via counter (≥100000), creates Tasks atomically; rejects 0 tasks, invalid dates, ClientManager/archived consultant.
- [✓] `PUT /api/contracts/{id}` upserts/archives Tasks inside one transaction; preserves Task IDs for unchanged rows.
- [✓] `PATCH /api/contracts/{id}/archive` and `/unarchive` toggle IsArchived without cascading to Tasks.
- [✓] `/admin/contracts` list page: six sortable columns, search, Show-archived toggle, AlertDialog on archive, Sonner toasts.
- [✓] `/admin/contracts/new` and `/$id` form: Card-wrapped, Customer + Consultant Selects, Tasks sub-form (add/remove rows), single submit.
- [✓] Customer options exclude archived; Consultant options exclude ClientManager.
- [✓] Seeder produces 5 Contracts × 2 Tasks; CounterSeeder sets contract counter to 100004 (next call returns 100005).
- [✓] `bun run check` passes; existing S1 tests still pass (59 unit + 39 integration).
- Note: Component tests omitted per project convention (see feedback_no_component_tests.md).

## Log

- 2026-05-15T08:30 — Read PLAN.md + explored codebase (Customer/User patterns, migrations, frontend structure)
- 2026-05-15T08:45 — Created Counter entity, ICounterService, CounterService, CounterConfiguration, CounterSeeder
- 2026-05-15T08:50 — Created Contract, ContractTask entities and EF configurations
- 2026-05-15T08:55 — Created ContractContracts.cs (DTOs/enums), ContractService, ContractEndpoints, ContractSeeder
- 2026-05-15T09:00 — Updated AppDbContext, Program.cs (services + seeders + endpoints)
- 2026-05-15T09:05 — Refactored CustomerService.CreateAsync to use ICounterService
- 2026-05-15T09:06 — Generated EF migration AddContractsAndCounters; API build succeeded
- 2026-05-15T09:10 — Updated CustomerServiceTests to inject ICounterService; updated CustomerEndpointsTests to seed Counters
- 2026-05-15T09:15 — All 42 unit + 25 integration (existing) tests pass
- 2026-05-15T09:20 — Wrote ContractServiceTests (17 tests); hit EF InMemory bug: adding to loaded navigation collection fails SaveChanges
- 2026-05-15T09:35 — Debugged: root cause is `collection.Add()` on already-tracked+loaded collection; fixed by using `_dbContext.ContractTasks.Add()` with explicit ContractId for new tasks in UpdateAsync
- 2026-05-15T09:40 — All 59 unit tests pass; wrote ContractEndpointsTests (14 tests); all 39 integration tests pass
- 2026-05-15T09:45 — Generated OpenAPI schema; created frontend feature layer (schemas, server, functions)
- 2026-05-15T09:50 — Created contracts routes: index.tsx (list + archived toggle), new.tsx, $id.tsx, -components/form.tsx (with Tasks sub-form)
- 2026-05-15T09:55 — `bun run check:fix && bun run check` passes; sidebar already had Contracts entry
