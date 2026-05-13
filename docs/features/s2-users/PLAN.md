# PLAN: S2 — Users (basic CRUD)

Source: feature.md

## Goal
Add Users CRUD with fields Name, Email, Role (Admin / User / ClientManager), IsArchived. Establishes the identity records that S6 OAuth/Entra ID will later authenticate against. No per-user leave configuration in this slice (deferred to S5).

## Approach

Mirrors the S1 Customers pattern in `packages/api/Modules/Customers/` and `packages/web/src/routes/admin/customers/`. Reuse shared components (TextField, FieldError, SortableHeader, TablePagination) — no duplication.

**API** (`packages/api/Modules/Users/`)
- Entity `User`: `Id` (Guid PK), `Name`, `Email`, `Role` (enum `UserRole { Admin, User, ClientManager }`), `IsArchived`. No business `Number`.
- EF configuration: table `Users`, unique index on `Email`, `Role` stored as string conversion, max-lengths Name(200)/Email(254). Register `UserConfiguration` in `AppDbContext`.
- New EF Core migration on `AppDbContext`.
- DTOs: `UserRequest` with DataAnnotations — Name `[Required, StringLength(200)]`, Email `[Required, EmailAddress, StringLength(254)]`, Role `[Required]`. `UserSort` enum (Name, Email, Role). `PagedUsers(IReadOnlyList<User> Items, int Total)`.
- Service: list (search across Name+Email+Role string, sort with logical role ordinal via switch — Admin=0, ClientManager=1, User=2 — direction-aware, paged), getById, create (check email uniqueness → return Result/throw with 409 mapping), update (same uniqueness check excluding self), archive, unarchive.
- Endpoints under `MapApiGroup("users")`: `GET /`, `GET /{id:guid}`, `POST /`, `PUT /{id:guid}`, `PATCH /{id:guid}/archive`, `PATCH /{id:guid}/unarchive`. POST/PUT use `ValidationFilter<UserRequest>`. Duplicate email returns `409 Conflict` with a problem-details body.
- Seeder `UserSeeder`: idempotent; inserts ~10 users (1 Admin, 2 ClientManagers, 7 Users) when table is empty. Called from `Program.cs` alongside customer seeding.

**Web** (`packages/web/src/routes/admin/users/`)
- Regenerate openapi-fetch types after API compiles; add `packages/web/src/api/users/index.ts` wrappers matching customer client shape.
- Routes:
  - `index.tsx` — list with debounced search, three `SortableHeader`s (Name/Email/Role), `TablePagination`. Archived users excluded server-side (only non-archived listed). Per-row Edit link + Archive button with `AlertDialog`.
  - `new.tsx` — renders `UserForm` with empty defaults (role defaults to `User`), `createUserFn` on submit.
  - `$id.tsx` — loader fetches by id; renders `UserForm` with `updateUserFn`.
- `-components/user-form.tsx`: Card-wrapped TanStack Form, Zod `userSchema` via `validators.onChange`. Name + Email use `TextField`; Role uses shadcn `Select` with options `Admin`, `Client Manager` (value `ClientManager`), `User`. Sonner toast on success, navigate back to list. Map server 409 → inline email error / toast.
- `-schemas.ts`: `userSchema` (Zod — name required, email required+email, role enum), `searchSchema` (search/sort/sortDirection/page).
- `-server.ts`: `fetchUsers`, `fetchUserById`, `createUserFn`, `updateUserFn`, `archiveUserFn` server functions wrapping the api client.
- Add shadcn `Select` component if not already installed (`bunx shadcn@latest add select` in `packages/web`).

**Tests**
- Backend: xUnit tests for `UserService` (sort/search/page, role ordinal sort, duplicate email 409, archive/unarchive) and endpoint smoke tests, in line with S1 coverage.
- No React component tests (per project convention). Note this omission in IMPL.md.

## Acceptance criteria
- `GET /api/users` supports `?search=` (substring on Name/Email/Role), `?sort=Name|Email|Role`, `?sortDirection=Asc|Desc`, `?page=`, `?pageSize=`; archived users excluded.
- Role sort uses logical order Admin → ClientManager → User (asc) regardless of stored-string lexicographic order.
- `POST /api/users` and `PUT /api/users/{id}` enforce required fields, valid email, and email uniqueness (409 on duplicate, excluding self on update).
- `PATCH /api/users/{id}/archive` and `/unarchive` toggle `IsArchived`.
- EF migration applied; unique index on Email present in `tsz.db`.
- Seeder produces ~10 users with mixed roles on a fresh database.
- `/admin/users` list page: search debounced, three sortable headers, pagination, archive AlertDialog with Sonner toast on success.
- `/admin/users/new` and `/admin/users/$id` save successfully, surface validation errors inline, surface duplicate-email 409 to the user.
- Backend test suite passes; `bun check` passes after schema regeneration.
