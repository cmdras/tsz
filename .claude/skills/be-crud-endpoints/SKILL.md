---
name: be-crud-endpoints
description: Invoke before writing or modifying .NET Minimal API CRUD endpoints (POST list/create, GET read, PUT update, PATCH archive/unarchive). Conventions for MapApiGroup, page/sort coalescing, Results<Ok, NotFound>, ValidationFilter.
user-invocable: false
---

Pairs with [[be-crud-service]] (DB work). Consumed by [[fe-data-access]] via the generated openapi client.

## Module layout

`packages/api/Modules/<Entities>/` contains:
- `<Entity>.cs` — EF entity, PK `Guid Id`, soft-delete `bool IsArchived`
- `<Entity>Configuration.cs` — `IEntityTypeConfiguration<>` for column constraints
- `<Entity>Contracts.cs` — `<Entity>Sort` enum, `Paged<Entities>` record, `<Entity>Request` DTO with `[Required]`/`[StringLength]`/`[EmailAddress]` data annotations
- `<Entity>Service.cs` — see [[be-crud-service]]
- `<Entity>Endpoints.cs` — this skill
- `<Entity>Seeder.cs` — dev/test seed data

Endpoints register via a static `Map(WebApplication app)` called from `Program.cs`.

## Operations

| Verb  | Path                       | Returns                       |
| ----- | -------------------------- | ----------------------------- |
| GET   | `/`                        | `Ok<Paged<Entity>>`           |
| GET   | `/{id:guid}`               | `Ok<Entity>` \| `NotFound`    |
| POST  | `/`                        | `CreatedAtRoute<Entity>`      |
| PUT   | `/{id:guid}`               | `Ok<Entity>` \| `NotFound`    |
| PATCH | `/{id:guid}/archive`       | `NoContent` \| `NotFound`     |
| PATCH | `/{id:guid}/unarchive`     | `NoContent` \| `NotFound`     |

No hard `DELETE` — soft-delete only.

## Conventions

- Use `app.MapApiGroup("entities")` (project extension), not raw `app.MapGet`.
- Optional query params must be **nullable** — Minimal API treats non-nullable params as required. Coalesce defaults inside the handler.
- Page defaults: page = `>0 ? value : 1`; pageSize = `>0 and <=100 ? value : 25`.
- Sort default: coalesce to the **first** value of the entity's `Sort` enum.
- Return `TypedResults.*`; declare typed result unions (`Results<Ok<T>, NotFound>`) on every handler that has a non-200 path.
- POST uses `CreatedAtRoute` and references the GET-by-id route — name it with `.WithName("GetEntityById")`.
- Apply `.AddEndpointFilter<ValidationFilter<EntityRequest>>()` to POST and PUT.
- Accept `CancellationToken` in every handler and forward it to the service.
- `Sort` enum order is meaningful: first value = default. Order entries to read naturally in a sort dropdown.
- Enums serialize as PascalCase strings via the global `JsonStringEnumConverter` — match that casing on the FE.

Canonical example: `packages/api/Modules/Customers/CustomerEndpoints.cs` and `CustomerContracts.cs`.
