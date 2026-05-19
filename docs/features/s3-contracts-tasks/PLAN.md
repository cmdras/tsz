# PLAN: S3 — Contracts + Tasks

Source: feature.md

## Goal

Ship admin CRUD for Contracts with a nested 1:N Tasks sub-form, copying S1's Customer patterns end-to-end. Extract the shared Counters service in the process and retrofit S1 onto it. Tasks become S7's Time Entry row primitive, so they get per-row soft-delete and stable identity.

## Approach

**Counters service.** Add `Common/Counters/ICounterService` with `NextAsync(string key, CancellationToken)` backed by a new `Counters` table (`Key` PK, `Value` int). Implementation uses a serializable transaction to read-and-increment atomically. Refactor `CustomerService.CreateAsync` to call `counters.NextAsync("customer")`, removing the inline `MaxAsync + 1` logic. New migration covers the Counters table and seeds rows for `customer` and `contract` keys at the current high-water mark.

**Backend Contract module.** Add `Modules/Contracts/{Contract.cs, Task.cs, ContractConfiguration.cs, TaskConfiguration.cs, ContractService.cs, ContractEndpoints.cs, ContractContracts.cs, ContractSeeder.cs}` mirroring `Modules/Customers/`. Contract: `Id` (Guid PK), `Number` (int, 6-digit via Counters key `contract`, starts 100000), `CustomerId`, `ConsultantId`, `Subject`, `StartDate`, `EndDate?`, `IsArchived`. Task: `Id` (Guid PK), `ContractId` FK, `Name`, `DayRate` (decimal), `Order` (int, server-assigned), `IsArchived`. Tasks live in their own DbSet (not EF-owned) so per-row archive and stable IDs survive Time Entry references; FK without cascade so archiving a Contract leaves Task rows queryable.

**Atomic Task save.** Contract `PUT` request body carries the full Task list. Server diffs inside a transaction: existing IDs update in place, missing IDs flip `IsArchived = true` (not deleted), new rows insert with the next `Order` value. `POST` creates Contract + initial Tasks in one transaction. Validation: ≥1 active Task on create/update, `DayRate > 0`, `Name` required and trimmed, `StartDate ≤ EndDate` when EndDate present, all Contract fields required except EndDate, Consultant must resolve to a non-archived User with `Role ≠ ClientManager`.

**Endpoints.** `MapApiGroup("contracts")` with `GET /` (paginate, search Subject/Customer.Name/Consultant.Name, sort on all six columns, `archived` filter), `GET /{id}`, `POST /`, `PUT /{id}`, `PATCH /{id}/archive`, `PATCH /{id}/unarchive`. ValidationFilter on mutations.

**Frontend feature folder.** `features/contracts/{contracts.schemas.ts, contracts.server.ts, contracts.functions.ts}` mirroring `features/customers/`. Zod schemas cover Contract + nested Task array. Server functions wrap openapi-fetch with `.inputValidator` and Zod.

**Frontend routes.** `routes/admin/contracts/{index.tsx, new.tsx, $id.tsx}` plus `-components/form.tsx`. List page: six sortable columns (Number, Customer, Subject, Consultant, StartDate, EndDate), search input, Show-archived toggle (S3-only, no S1 retrofit), AlertDialog on archive, Sonner toasts, `TablePagination`. Form page: Card-wrapped TanStack Form with Customer + Consultant shadcn Select pickers (non-archived rows only, label format `100001 — Name` for Customer / `Name` for Consultant, fetched once via loader), then a Tasks sub-form rendered as add/remove rows below the Contract fields, single shared submit. Picker option lists pulled via server-fn loaders on route entry.

**Seed.** `ContractSeeder` creates 5 contracts (Numbers 100000–100004 via Counters) with 2 Tasks each, drawing Customer + Consultant FKs from existing seed pools. Counters table seeded so the next call returns 100005.

## Acceptance criteria

- `ICounterService` exists in `Common/Counters/`; `CustomerService.CreateAsync` uses it; S1's inline `MaxAsync + 1` is gone.
- Counters table migration applied; rows for `customer` and `contract` initialized to current high-water marks.
- `GET /api/contracts` paginates, searches Subject/Customer/Consultant, sorts on all six columns, hides archived unless `archived=true` query param.
- `POST /api/contracts` assigns `Number` via Counters key `contract` (≥100000), creates Tasks atomically; rejects payloads with 0 active Tasks, `DayRate ≤ 0`, `StartDate > EndDate`, missing required fields, or Consultant with role `ClientManager` or archived.
- `PUT /api/contracts/{id}` upserts/archives Tasks to match request body inside one transaction; preserves Task IDs for unchanged rows.
- `PATCH /api/contracts/{id}/archive` and `/unarchive` toggle `IsArchived`; do not cascade to Tasks.
- `/admin/contracts` list page renders six sortable columns, search input, Show-archived toggle, AlertDialog confirmation on archive, Sonner success/error toasts.
- `/admin/contracts/$id` and `/admin/contracts/new` render one Card-wrapped form with Customer + Consultant Selects, Tasks sub-form with add/remove rows, single submit; client-side Zod enforces ≥1 Task, `DayRate > 0`, `StartDate ≤ EndDate`.
- Customer and Consultant Select options exclude archived rows; Consultant options exclude `ClientManager` role.
- Seeder produces 5 Contracts × 2 Tasks on fresh DB; subsequent contract creation continues from 100005.
- `bun run check` passes; `bun run dev:api` starts without errors; existing S1 tests still pass after Counters retrofit.
