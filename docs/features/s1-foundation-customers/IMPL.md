# IMPL: S1 — Foundation + Customers

Plan: ./PLAN.md
Started: 2026-05-12T12:00:00Z
Finished: 2026-05-12T14:13:00Z

## Acceptance check
- [✓] `/admin/customers` lists 5 seeded rows with numbers 100000–100004, sorted by Number ASC — verified via API, UI routes wired
- [✓] `POST /api/customers` without `Number` in body returns 201 with server-assigned next Number; the 6th customer gets 100005 — smoke tested
- [✓] `PUT /api/customers/{id}` rejects/ignores `Number` and `Id` mutation (DTO omits both) — PUT returned number:100000 unchanged
- [✓] Archive flow: AlertDialog confirms → `PATCH /archive` → row disappears from default list → reappears when "Show archived" Switch is on — API verified; UI uses AlertDialog + router.invalidate()
- [✓] Unarchive flow: direct `PATCH /unarchive`, no confirm, toast on result — API 204, UI direct button
- [✓] Search input filters server-side on `Name` + `ContactName` substring; URL reflects `?search=` and `?includeArchived=` and survives reload — search=alpha returns only Alpha; URL via validateSearch
- [✓] Invalid email blocks form submit; `FieldError` appears only after touch — customerSchema enforces email, FieldError uses isTouched
- [✓] Sonner toast fires on every mutation success and error — toast.success/error in form onSubmit and list handlers
- [✓] `bun check` clean; `bun run test` green including existing animal tests — 30/30 tests pass
- [✓] `packages/web/src/api/schema.ts` regenerated; openapi-fetch types compile — regenerated, types compile clean

## Log
- 2026-05-12T12:00Z — Created IMPL.md
- 2026-05-12T12:05Z — Added AppDb connection string to appsettings.json
- 2026-05-12T12:06Z — Created AppDbContext, Customer, CustomerConfiguration, CustomerContracts, CustomerService, CustomerEndpoints
- 2026-05-12T12:07Z — Updated Program.cs: registered AppDbContext, MigrateAsync, inline customer seeding
- 2026-05-12T12:08Z — Generated EF Core migration via dotnet-ef: Migrations/AppDb/20260512120744_Initial.cs
- 2026-05-12T12:10Z — Installed shadcn components: sonner, switch, alert-dialog
- 2026-05-12T12:11Z — Mounted <Toaster /> in __root.tsx
- 2026-05-12T12:12Z — Deleted flat routes/admin/customers.tsx; created customers/ folder structure
- 2026-05-12T12:13Z — Created -schemas.ts, -server.ts, -components/customer-form.tsx
- 2026-05-12T12:14Z — Created index.tsx (list), $id.tsx (edit), new.tsx (create)
- 2026-05-12T12:15Z — Created api/customers/index.ts with openapi-fetch wrappers
- 2026-05-12T12:16Z — Created -schemas.spec.ts; all 8 schema tests pass
- 2026-05-12T12:20Z — Fixed bool? includeArchived (was bool, required query param); rebuilt API
- 2026-05-12T12:21Z — Regenerated schema.ts; bun check clean; 30/30 tests pass
- 2026-05-12T14:13Z — Note: form-behavior tests skipped per project conventions (no React component tests)

## Errors encountered

1. **`dotnet-ef` not installed** — `dotnet ef migrations add` failed immediately; the global tool was not on PATH. Had to run `dotnet tool install --global dotnet-ef` and then export `~/.dotnet/tools` to PATH before the migration command worked. **Fix for PLAN:** add a pre-flight note that `dotnet-ef` must be available, or include the install step.

2. **`bool includeArchived` became a required query parameter** — Minimal API bound `bool includeArchived` as a mandatory query string param (no default), so `GET /api/customers` returned 400 without it. First fix attempt used `bool includeArchived = false, CustomerService service = null!` (bad pattern). Corrected to `bool? includeArchived` with null-coalescing in the service call. **Fix for PLAN:** specify `bool?` explicitly, or note that defaulted booleans need nullable typing in minimal APIs.

3. **Stale binary served by VS Code `dotnet watch run`** — After fixing the endpoint and rebuilding, curl still hit the old response. Root cause: VS Code had a `dotnet watch run` process already holding port 5204; our `dotnet run --no-build` spawned a second process that never bound the port. Wasted two restart cycles before identifying the watch process and killing it by pid. **Fix for process:** check `ss -tlnp | grep 5204` before assuming a fresh server is running.

4. **`bunx shadcn@latest add` is interactive** — Adding multiple components in one invocation prompted "overwrite existing file?" for each already-present component (button.tsx), blocking the shell. Required two separate runs and piping `printf 'N\n'` to answer the prompt. **Fix for PLAN:** add components one at a time, or note that the `--overwrite` / `--yes` flags do not suppress per-file prompts.

5. **Freshly scaffolded shadcn files and regenerated `schema.ts` fail `bun check`** — `alert-dialog.tsx`, `sonner.tsx`, `switch.tsx`, and `schema.ts` all needed a `vp check --fix` pass after creation. Not a code defect, but adds a mandatory step after every shadcn add and every schema regeneration. **Fix for PLAN:** append `bun run check:fix` to the shadcn and `gen:api` steps.
