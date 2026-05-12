# PLAN: S1 — Foundation + Customers

Source: ./feature.md

## Goal

Add `AppDbContext` + `tsz.db` (EF migrations) and a full Customer admin vertical. First instance of every admin pattern S2–S10 will copy: single DbContext, GUID PK + business `Number`, soft-delete via PATCH archive/unarchive, Card-wrapped form, Sonner toasts.

## Approach

### API

- New `packages/api/Common/Database/AppDbContext.cs` with `DbSet<Customer>`. Animals stays on its own `AnimalDbContext`.
- Connection string `AppDb` in `appsettings.json` → `Data Source=tsz.db`. Register `AddDbContext<AppDbContext>` in `Program.cs`.
- EF Core migrations under `Migrations/AppDb/`. Initial migration creates `Customers`. Animals migrations stay at `Migrations/` root, untouched. Generate via `dotnet ef migrations add Initial -c AppDbContext -o Migrations/AppDb`.
- New `packages/api/Modules/Customers/` mirrors the animals module structure:
  - `Customer.cs` — entity: `Id` (Guid, PK), `Number` (int, unique, server-assigned), `Name`, `Street`, `Zip`, `City`, `Country`, `ContactName`, `ContactEmail`, `IsArchived` (default false).
  - `CustomerConfiguration.cs` — unique index on `Number`; max-length annotations.
  - `CustomerContracts.cs` — `CreateCustomerRequest` and `UpdateCustomerRequest` (DataAnnotations: `[Required]`, `[StringLength]`, `[EmailAddress]`). Neither includes `Id` or `Number` — server-assigned/immutable.
  - `CustomerService.cs` — `GetAllAsync(search, includeArchived)`, `GetByIdAsync(id)`, `CreateAsync(req)`, `UpdateAsync(id, req)`, `ArchiveAsync(id)`, `UnarchiveAsync(id)`. `CreateAsync` computes Number inline: `(_db.Customers.Max(c => (int?)c.Number) ?? 99999) + 1`, then inserts.
  - `CustomerEndpoints.cs` — `app.MapApiGroup("customers")` with: `GET /` (with `?search=&includeArchived=` query binding), `GET /{id:guid}`, `POST /` (+ `ValidationFilter<CreateCustomerRequest>`), `PUT /{id:guid}` (+ `ValidationFilter<UpdateCustomerRequest>`), `PATCH /{id:guid}/archive`, `PATCH /{id:guid}/unarchive`. **No DELETE.**
- No `[Authorize]` in S1. S6 retroactively applies it once Entra is wired.
- Server-side `?search=` matches substring on `Name` OR `ContactName` (case-insensitive). `?includeArchived=false` by default; `true` includes archived rows.
- `Program.cs`: replace `EnsureCreated`/AnimalSeeder block to also resolve `AppDbContext`, call `MigrateAsync()`, and inline-seed 5 customers (Alpha…Echo, Belgian addresses, numbers 100000–100004) iff `Customers` table is empty. Animals seeding stays.

### Web

- `bunx shadcn@latest add sonner switch alert-dialog` from `packages/web`.
- Mount `<Toaster />` in `routes/__root.tsx` (next to existing chrome).
- Delete flat `routes/admin/customers.tsx`. Create folder `routes/admin/customers/`:
  - `index.tsx` — list page. `validateSearch` with `searchSchema` (search, includeArchived). `loaderDeps` passes both into `loader`. shadcn Table columns: Number (6-digit display via `String(n).padStart(6, '0')`), Name, Contact, Email, City, Country, Archive button. Sort by Number ASC. Search Input above table (debounced 300ms updates URL via `navigate({ search })`). "Show archived" Switch toggles `includeArchived`. "New customer" button → `/admin/customers/new`. Per-row archive: AlertDialog → PATCH `/archive` → toast + `router.invalidate()`. Per-row unarchive (visible on archived rows when Switch on): direct PATCH → toast.
  - `$id.tsx` — edit page. Loader fetches by id. Renders `<CustomerForm initial={customer} onSubmit={updateServerFn} />`.
  - `new.tsx` — create page. Renders `<CustomerForm initial={defaults} onSubmit={createServerFn} />`. Defaults: `country: 'Belgium'`, everything else empty.
  - `-components/customer-form.tsx` — shared Card-wrapped form. TanStack Form with `validators.onChange: customerSchema`. Fields: Name, ContactName, ContactEmail (type=email), Street, Zip, City, Country (all plain `<Input>` + `<Label>` + `<FieldError>`). Submit button disabled until valid. On submit success → toast + navigate back to list.
  - `-schemas.ts` — `customerSchema` (zod: all strings required, ContactEmail `.email()`, Country `.min(1)`) and `searchSchema` (`search: z.string().optional()`, `includeArchived: z.boolean().optional()`).
  - `-server.ts` — `createServerFn` wrappers per CRUD op, each `.inputValidator(zodSchema)`.
- `packages/web/src/api/customers/index.ts` — openapi-fetch wrappers mirroring `animals/index.ts`: `getCustomers(search, includeArchived)`, `getCustomerById(id)`, `createCustomer(body)`, `updateCustomer(id, body)`, `archiveCustomer(id)`, `unarchiveCustomer(id)`.
- Regenerate `packages/web/src/api/schema.ts` after the API endpoints compile.

### Tests

- Vitest only. Cover `customerSchema`: required fields, email format, country required, valid case passes.
- Skip form-behavior tests (per memory: no React component tests). Note the omission in IMPL.md.
- Existing animal tests must still pass.

### Out of scope

Pagination, multi-sort, client manager, VAT/phone/GTC/start-date, real auth, hard DELETE, country Select with "Other" branch, xUnit tests, form-behavior tests, CustomerSeeder class.

## Acceptance criteria

- `/admin/customers` lists 5 seeded rows with numbers 100000–100004, sorted by Number ASC.
- `POST /api/customers` without `Number` in body returns 201 with server-assigned next Number; the 6th customer gets 100005.
- `PUT /api/customers/{id}` rejects/ignores `Number` and `Id` mutation (DTO omits both).
- Archive flow: AlertDialog confirms → `PATCH /archive` → row disappears from default list → reappears when "Show archived" Switch is on.
- Unarchive flow: direct `PATCH /unarchive`, no confirm, toast on result.
- Search input filters server-side on `Name` + `ContactName` substring; URL reflects `?search=` and `?includeArchived=` and survives reload.
- Invalid email blocks form submit; `FieldError` appears only after touch.
- Sonner toast fires on every mutation success and error.
- `bun check` clean; `bun run test` green including existing animal tests.
- `packages/web/src/api/schema.ts` regenerated; openapi-fetch types compile.
