# PLAN: Repository pattern rollout

Source: feature.md

## Goal

Roll out the `Customers` repository pattern across `Contracts`, `Users`, `LeaveTypes`, and `UserLeaveAllowances`. Each aggregate gains an `IXRepository` + `XRepository` (EF only, returns entities, owns its `SaveChangesAsync`) and a thinned `XService` (validation, mapping, cross-aggregate composition). Pure refactor — no behavior change, no endpoint or contract change.

## Approach

**Repository surface (per module).** Mirror `ICustomerRepository`: `GetAllAsync(search, sort, sortDirection, page, pageSize, ...)`, `GetByIdAsync(id)`, `CreateAsync(request)`, `UpdateAsync(id, request)`, `ArchiveAsync(id)`, `UnarchiveAsync(id)`. Repos return entities (or `(IReadOnlyList<T>, int Total)` for paged reads) and call `SaveChangesAsync` themselves. No DTO mapping inside repos. No `using Api.Common.Exceptions` in any repository file.

**Contracts.** `IContractRepository` + `ContractRepository` absorbs all `_dbContext.Contracts`/`_dbContext.ContractTasks` access from `ContractService`, including the `Include(Customer)`/`Include(Consultant)`/`Include(Tasks)` queries, the `Order` bookkeeping on tasks, `LoadReferencesAsync`, and the existing `BeginTransactionAsync(Serializable)` + `MAX(Number) + 1` block (moved from service to `CreateAsync`). `ContractService` keeps `ValidateRequestAsync`, the task reconciliation diff (kept/added/archived), the `BuildTask`/`ToResponse` mapping helpers, and `InvalidContractRequestException`. Cross-aggregate validation calls go through `ICustomerRepository.GetByIdAsync` and a new `IUserRepository.GetByIdAsync` (which must return the `User` entity, not the existing `UserResponse?`).

**Users.** `IUserRepository` + `UserRepository` owns `_dbContext.Users` (paged list with role-search + role-sort key, `FindAsync`, email-existence checks via `AnyAsync`, archive helpers). `IUserLeaveAllowanceRepository` + `UserLeaveAllowanceRepository` owns `_dbContext.UserLeaveAllowances` with fine-grained CRUD: `GetForUserAndYearAsync(userId, year)`, `AddAsync(entity)`, `RemoveAsync(id)` (or `RemoveRangeAsync(ids)`), and exposes `SaveChangesAsync()` via its own methods. `UserService` composes `IUserRepository` + `IUserLeaveAllowanceRepository` + `ILeaveTypeRepository`; it owns `SeedLeaveAllowancesAsync`, the kept/added/removed diff in `UpdateAsync`, the joined read in `GetByIdAsync` (call `_userRepo.GetByIdAsync` + `_allowanceRepo.GetForUserAndYearAsync` + `_leaveTypeRepo.GetByIdsAsync` then build the `UserResponse` inline), `GetOrProvisionAsync`, and the existing exceptions (`DuplicateEmailException`, `UnknownLeaveTypeException`, `DuplicateUserLeaveAllowanceException`). `CreateAsync`, `UpdateAsync`, and `GetOrProvisionAsync` wrap composed writes in `BeginTransactionAsync(IsolationLevel.Serializable)` at the service layer; repos still call `SaveChangesAsync` per method (ambient tx is respected).

**LeaveTypes.** `ILeaveTypeRepository` + `LeaveTypeRepository` absorbs all `_dbContext.LeaveTypes` access including the duplicate-name `AnyAsync` checks (exposed as a repo method like `ExistsByNameAsync(name, excludeId = null)`). `LeaveTypeService` keeps `ToResponse` mapping and `DuplicateLeaveTypeNameException`.

**UserLeaveAllowances.** Repository exists purely to be composed by `UserService`. No new `XService` class; the module already has only contracts + configuration + entity. `UserLeaveAllowancesModule` (new, mirroring `CustomersModule`) registers `IUserLeaveAllowanceRepository -> UserLeaveAllowanceRepository`; wire into `Program.cs`.

**Module registration.** Each `XModule.AddXModule` registers its `IXRepository -> XRepository` as scoped, then its service. No changes to endpoint mapping.

**Tests.** For each refactored module, mirror Customers: keep the existing `XServiceTests.cs` (construct `XService` with real composed repos against in-memory `AppDbContext`, matching `CustomerServiceTests.CreateService`) and add a new `XRepositoryTests.cs` exercising the repository surface (filter/search/sort/pagination, `GetByIdAsync` not-found, `CreateAsync` including the `MAX+1` transaction for `ContractRepository`, archive/unarchive). For `UserLeaveAllowances`, add `UserLeaveAllowanceRepositoryTests.cs`. `UserLeaveAllowanceServiceTests.cs` keeps its current name (asserts `UserService` leave behaviors) — out of scope to rename.

**Integration tests.** No changes — they go through endpoints which are untouched.

## Acceptance criteria

- No `AppDbContext` field on `ContractService`, `UserService`, or `LeaveTypeService`.
- `IContractRepository` + `ContractRepository`, `IUserRepository` + `UserRepository`, `ILeaveTypeRepository` + `LeaveTypeRepository`, `IUserLeaveAllowanceRepository` + `UserLeaveAllowanceRepository` all exist and are registered as scoped via their respective `XModule.AddXModule`.
- `ContractService` constructor takes `IContractRepository` + `ICustomerRepository` + `IUserRepository`. `UserService` constructor takes `IUserRepository` + `IUserLeaveAllowanceRepository` + `ILeaveTypeRepository`. `LeaveTypeService` constructor takes `ILeaveTypeRepository`.
- `ContractRepository.CreateAsync` opens its own `BeginTransactionAsync(IsolationLevel.Serializable)` for `MAX(Number) + 1` + insert; no `BeginTransactionAsync` remains in `ContractService`.
- `UserService.CreateAsync`, `UpdateAsync`, and `GetOrProvisionAsync` open `BeginTransactionAsync(IsolationLevel.Serializable)` around composed repo calls.
- Domain exceptions (`InvalidContractRequestException`, `DuplicateLeaveTypeNameException`, `DuplicateEmailException`, `UnknownLeaveTypeException`, `DuplicateUserLeaveAllowanceException`) remain in service files; `grep -r "Exception" packages/api/Modules/**/I*Repository.cs packages/api/Modules/**/*Repository.cs` finds no domain exception references.
- New test files exist: `ContractRepositoryTests.cs`, `UserRepositoryTests.cs`, `LeaveTypeRepositoryTests.cs`, `UserLeaveAllowanceRepositoryTests.cs`. Existing `ContractServiceTests.cs`, `UserServiceTests.cs`, `LeaveTypeServiceTests.cs`, `UserLeaveAllowanceServiceTests.cs` updated to construct services with real composed repos.
- Endpoints, contracts, EF configurations, seeders, migrations are byte-identical to pre-refactor (except for any DI wiring inside `Program.cs` to register `UserLeaveAllowancesModule`).
- `dotnet build` succeeds; `dotnet test` is green for `api.tests` and `api.tests.integration`.
