---
name: be-crud-service
description: Invoke before writing or modifying a CRUD service class (XService.cs). Patterns for GetAllAsync (filter + search + sort + page), GetByIdAsync, CreateAsync, UpdateAsync, ArchiveAsync/UnarchiveAsync.
user-invocable: false
---

Paired with [[be-crud-endpoints]] — endpoints inject and call this service.

## Structure

- Constructor-inject `AppDbContext` only.
- Every public method takes `CancellationToken cancellationToken = default` and forwards it to every EF call.
- Return `<Entity>Response` DTOs from all public methods — map via a private `static <Entity>Response ToResponse(<Entity> entity)` method defined at the bottom of the service class.
- `GetAllAsync` returns `Paged<Entities>(Items, Total)` (record defined in `<Entity>Contracts.cs`).
- `ArchiveAsync` / `UnarchiveAsync` return `bool` — `false` means "not found".

## GetAllAsync pipeline

Order matters: **archived filter → search filter → sort → count → paginate**.

- Start from `_dbContext.Entities.Where(entity => !entity.IsArchived)`.
- Search: trim + lowercase the term; combine searchable fields with `||` and `.ToLower().Contains(term)`. Skip the block when `IsNullOrWhiteSpace`.
- Sort: a `switch` on the `Sort` enum, with the `_` default matching the enum's first value. Branch on `sortDirection == SortDirection.Desc` for asc/desc.
- Count BEFORE `Skip/Take`. Items: `Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct)`.
- Sorting by non-column values (e.g. enum priority): hoist a static `Expression<Func<Entity, int>>` and pass it to `OrderBy(KeyExpression)`.

## Other methods

- `GetByIdAsync` → `_dbContext.Entities.FindAsync([id], ct).AsTask()`. `FindAsync` checks the change tracker first; preferred over `FirstOrDefaultAsync` for single-PK lookups.
- `CreateAsync` → run invariant checks (see below), build entity with `Id = Guid.NewGuid()`, `AddAsync`, `SaveChangesAsync`, return the entity.
- `UpdateAsync` → `FindAsync`; if `null` return `null`; run invariant checks; mutate fields from request; `SaveChangesAsync`; return entity.
- `ArchiveAsync` / `UnarchiveAsync` → `FindAsync`; if `null` return `false`; set `IsArchived`; `SaveChangesAsync`; return `true`.

## Entity-specific invariants

Place invariant checks at the top of `CreateAsync` / `UpdateAsync`. Throw a typed domain exception that extends `DomainException` (e.g. `DuplicateEmailException() : DomainException("...", 409)`). `GlobalExceptionHandler` catches it automatically — no try/catch in endpoints.

- **Uniqueness**: `AnyAsync` before insert; on update exclude the current row (`other.Id != id`).
- **Generated sequence numbers** (e.g. 6-digit `Number`): wrap Create in `BeginTransactionAsync(IsolationLevel.Serializable, ct)`, compute `MaxAsync(...) + 1`, commit at end. See `CustomerService.CreateAsync` for the working pattern.

Canonical examples: `packages/api/Modules/Customers/CustomerService.cs` (sequence number + transaction) and `packages/api/Modules/Users/UserService.cs` (uniqueness check + expression-based sort).
