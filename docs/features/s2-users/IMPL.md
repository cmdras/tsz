# IMPL: S2 — Users (basic CRUD)

Plan: PLAN.md
Started: 2026-05-13T11:00:00Z
Finished: 2026-05-13T11:35:00Z

## Acceptance check

- [✓] `GET /api/users` supports `?search=`, `?sort=Name|Email|Role`, `?sortDirection=Asc|Desc`, `?page=`, `?pageSize=`; archived excluded
- [✓] Role sort uses logical order Admin → ClientManager → User (asc) via CASE WHEN expression in LINQ
- [✓] `POST` and `PUT` enforce required fields, valid email, and email uniqueness (409 on duplicate, excluding self on update)
- [✓] `PATCH /api/users/{id}/archive` and `/unarchive` toggle `IsArchived`
- [✓] EF migration `AddUsers` applied; unique index on Email present in `tsz.db`
- [✓] Seeder produces 10 users (1 Admin, 2 ClientManagers, 7 Users) when table is empty
- [✓] `/admin/users` list: search debounced, three sortable headers (Name/Email/Role), pagination, archive AlertDialog with Sonner toast
- [✓] `/admin/users/new` and `/admin/users/$id` save successfully, surface validation errors inline, surface duplicate-email 409 as toast
- [✓] Backend test suite passes (29 unit + 18 integration = 47 total); `bun check` passes

## Log

- 2026-05-13T11:00:00Z — read S1 Customers patterns; wrote IMPL.md header
- 2026-05-13T11:05:00Z — created User.cs, UserContracts.cs (UserRole, UserSort, UserSortDirection, DuplicateEmailException, PagedUsers, UserRequest), UserConfiguration.cs, UserService.cs, UserEndpoints.cs, UserSeeder.cs
- 2026-05-13T11:08:00Z — added UserSortDirection to Users namespace (avoids cross-module dep on Customers.SortDirection); SortDirection renamed UserSortDirection throughout
- 2026-05-13T11:09:00Z — updated AppDbContext, Program.cs; API build succeeded
- 2026-05-13T11:10:00Z — generated EF Core migration AddUsers; applied migration to tsz.db
- 2026-05-13T11:12:00Z — started API, regenerated schema.ts with bun run gen:api; confirmed UserRole/UserSort/UserSortDirection/PagedUsers/UserRequest in schema
- 2026-05-13T11:15:00Z — created api/users/index.ts client wrapper
- 2026-05-13T11:16:00Z — replaced users.tsx placeholder with Outlet layout; created users/ directory routes (-schemas.ts, -server.ts, -components/user-form.tsx, index.tsx, new.tsx, $id.tsx)
- 2026-05-13T11:18:00Z — installed shadcn Select component; bun check:fix passes
- 2026-05-13T11:20:00Z — wrote UserServiceTests (17 cases) and UserEndpointsTests (12 cases)
- 2026-05-13T11:22:00Z — fixed MigrateAsync InMemory incompatibility in Program.cs (conditional on IsRelational())
- 2026-05-13T11:25:00Z — fixed JSON string enum deserialization in integration tests
- 2026-05-13T11:28:00Z — fixed GetUsers_ArchivedUsers_ExcludedFromList — seeder has "Jack User" which matched search; used unique GUID-based name
- 2026-05-13T11:30:00Z — all 29 unit tests pass; all 18 integration tests pass; bun check passes

## Notes

- No React component tests per project convention
- `UserSortDirection` enum defined in Users namespace rather than reusing `SortDirection` from Customers, to avoid cross-module dependency; both have same Asc/Desc values
- 409 duplicate email propagated through TanStack Start server function boundary via `Error('EMAIL_ALREADY_IN_USE')` message sentinel; shown as toast in user-form
