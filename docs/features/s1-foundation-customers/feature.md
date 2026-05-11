# S1 — Foundation + Customers

First data slice on top of S0's chrome + stub-auth scaffold. Bundles two things: (a) the persistence foundation (`AppDbContext`, EF Core migrations, Counters service) that every later feature writes through, and (b) Customer CRUD as the first vertical that exercises the whole stack. The patterns established here are copied verbatim by S2–S10.

Spec source: `../poc-tsz/docs/product/{requirements.md,architecture.md}`, customer fields scoped to the "Customers" section (Prio 1 only — no Client manager, no VAT/contract metadata).

## Scope

### Sidebar entry

- The Admin section's `Customers` link in the sidebar (placeholder from S0) is wired to `/admin/customers` and shows the real list page. The S0 placeholder leaf `routes/admin/customers.tsx` is deleted and replaced by the `routes/admin/customers/` folder described below.
- Endpoint role-gating: admin endpoints require Admin role server-side (`[Authorize(Roles="Admin")]` on the route group, or equivalent). First instance lands here; every later admin module copies it.

### Foundation: AppDbContext

- New `AppDbContext` in `packages/api/Common/Database/` registering `DbSet<Customer>` and `DbSet<Counter>`. Every later module's entity configuration registers into this same context.
- Animals stays on its own `AnimalDbContext` + `animals.db` file — untouched until S11.
- New SQLite file: `tsz.db`. Connection string in `appsettings.json` under a distinct key.
- EF Core migrations (not just `EnsureCreated`). Initial migration generated for Customers + Counters.

### Foundation: Counters service

- Shared infrastructure under `packages/api/Common/Counters/`:
  - `Counter` entity: `Key` (string, PK), `Value` (int).
  - `ICounterService` with `Task<int> NextAsync(string key, CancellationToken ct)` that increments `Value` and returns the new value inside a transaction (race-safe under SQLite's serialized writes).
- S1 consumer: `customer` key. Starts at 100000. Seeded customers consume numbers, so 5 seeded rows = 100000–100004, next real customer = 100005.
- Contracts (S3) will get a separate counter strategy — TBD in S3 plan. Don't presume Contracts use this service.

### Customer entity

Fields:

- `Id` (Guid, PK, server-assigned)
- `Number` (int, unique, server-assigned via Counters service, 6 digits)
- `Name` (string, required)
- `Street`, `Zip`, `City`, `Country` (strings; country stored as ISO 3166-1 alpha-2 or a stable short code)
- `ContactName` (string)
- `ContactEmail` (string, email-validated)
- `IsArchived` (bool, default false)

No client-manager assignment (Prio 2). No VAT, phone, GTC status, send-invoice-by, or start-date (mock-only / out of scope for S1).

### Customer API (`/api/customers`)

All endpoints role-gated to Admin server-side.

- `GET /api/customers?search=...&includeArchived=true|false` — server-side substring filter on `Name` + `ContactName`; archived hidden by default.
- `GET /api/customers/{id:guid}` — by id.
- `POST /api/customers` — create. Server assigns `Id` (GUID) + `Number` (next counter value). Returns 201 with the created entity.
- `PUT /api/customers/{id:guid}` — full update (`Number` and `Id` not mutable).
- `PATCH /api/customers/{id:guid}/archive` — sets `IsArchived = true`.
- `PATCH /api/customers/{id:guid}/unarchive` — sets `IsArchived = false`.
- **No hard DELETE.**

`Common/Filters/ValidationFilter<T>` covers request validation on POST/PUT.

### Customer web (`/admin/customers/*`)

Follow the animals scaffold structure but Card-wrapped (shadcn Card + CardHeader + CardContent + CardFooter). Accepted divergence from animals — animals deletes in S11.

- `routes/admin/customers/index.tsx` — list page.
  - shadcn Table with columns: Number, Name, Contact, Email, City, Country, [archive button].
  - Default sort: Number ASC.
  - Search input above the table; debounced; drives the `?search=` query param via loader deps so navigations update the URL.
  - `Show archived` shadcn Switch above the table → toggles the `?includeArchived=` param.
  - `New customer` button → `/admin/customers/new`.
  - Archive button per row → shadcn AlertDialog confirm → PATCH `/archive` → toast + router invalidate. Unarchive button on archived rows → direct PATCH (no confirm) → toast.
- `routes/admin/customers/$id.tsx` — edit page (Card form).
- `routes/admin/customers/new.tsx` — create page (Card form, same component).
- `routes/admin/customers/-components/customer-form.tsx` — shared form. shadcn Card. Fields: Name (Input), Contact name (Input), Contact email (Input type=email), Street (Input), Zip (Input), City (Input), Country (shadcn Select: Belgium default + "Other" → text Input visible only when "Other"). TanStack Form + Zod via `validators.onChange: customerSchema`. `FieldError` per field.
- `routes/admin/customers/-schemas.ts` — `customerSchema` (shared between form and server-fn) + `searchSchema` for list query params.
- `routes/admin/customers/-server.ts` — `createServerFn` wrappers per API call, all with `.inputValidator(zodSchema)`.
- `api/customers/index.ts` — openapi-fetch wrappers (mirrors `api/animals/index.ts`).

### Notifications

- shadcn Sonner (`sonner` package). Toast on every mutation result — create/edit/archive/unarchive (success + error).
- AlertDialog only on archive (unarchive is undo-friendly).

### Seed data

- 5 placeholder customers via a `CustomerSeeder` (mirrors `AnimalSeeder` pattern). Names like `Customer Alpha`, `Customer Bravo`, …, `Customer Echo`. Belgian addresses. Numbers 100000–100004 (consumed via Counters service so the counter row reflects last-used).

### Tests (Vitest only)

- Zod schemas: required fields, email format, country defaults.
- Form behavior: invalid fields show `FieldError` only after touch; submit gated by validity.
- Defer xUnit / API integration tests until a later slice introduces them.

### OpenAPI codegen

- Regenerate `packages/web/src/api/schema.ts` after the C# entity/endpoints land. Standard pipeline.

## Out of scope for S1

- Pagination (5 seed rows + small expected set in early use).
- Multi-column sorting.
- Client manager on Customer (Prio 2).
- VAT, phone, GTC status, send-invoice-by, start-date (mock-only or out of scope).
- Bulk archive / bulk anything.
- xUnit / integration tests.
- Real auth (Entra) — deferred until post-Prio-1.
- Hard DELETE endpoint.

## Conventions established here (copied by S2–S10)

1. Single `AppDbContext`; new modules register `IEntityTypeConfiguration<T>` into it.
2. GUID PK + optional 6-digit business `Number` (Customers only — Contracts/Tasks/etc. follow their own slice's decisions).
3. Counters service in `Common/Counters/` for any sequential-number generation.
4. Soft-delete = `IsArchived` flag + PATCH `/archive` + PATCH `/unarchive`. No hard DELETE on admin entities.
5. Admin server-side role gating via `[Authorize]`-equivalent on the route group.
6. Admin routes under `/admin/<entity>/{index,$id,new}` with Card-wrapped forms.
7. List page = shadcn Table + search input + "Show archived" Switch + per-row archive button.
8. Shared form component in `routes/admin/<entity>/-components/<entity>-form.tsx`; Zod schema in `-schemas.ts`; server functions in `-server.ts`.
9. Toasts via Sonner on every mutation; AlertDialog confirm only on archive.

## Open / plan-time questions for `/piv-plan` to surface

- Exact storage type for `Number` — int (recommended; 100000–999999 fits) vs string. Display always 6 digits.
- Country code storage — ISO alpha-2 (`BE`) vs full name (`Belgium`)? Affects how the Select renders and how "Other" → free text round-trips.
- Exact role-gating mechanism in ASP.NET — `[Authorize(Roles="Admin")]` on the route group, or a custom endpoint filter?
- File location for the AppDbContext (`Common/Database/AppDbContext.cs` vs `AppDbContext.cs` at project root).
- Does the `Country` field's "Other" path block submit until a free-text value is entered? (Probably yes — the Zod schema enforces a non-empty country.)
