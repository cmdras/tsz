---
name: fe-data-access
description: Invoke before writing or modifying `<entity>.schemas.ts` (Zod entity + search schemas, sortSlugs), `<entity>.server.ts` (openapi-fetch client wrappers), or `<entity>.functions.ts` (TanStack server functions) for an admin entity.
user-invocable: false
---

Consumed by [[fe-form]] (validators) and [[fe-list]] (loader + mutations). Mirrors backend contracts from [[be-crud-endpoints]].

## Files

Three feature files under `src/features/<entity>/`:

- `<entity>.schemas.ts` — Zod entity schema, `sortSlugs` map, `searchSchema`. Source of truth for input types.
- `<entity>.server.ts` — openapi-fetch client wrappers (`getEntities`, `getEntityById`, `createEntity`, `updateEntity`, `archiveEntity`) plus the entity types re-exported from `#/api/schema`.
- `<entity>.functions.ts` — five `createServerFn` exports wrapping the client functions in `<entity>.server.ts`.

## `<entity>.schemas.ts` rules

- `entitySchema` is a `z.object({...})`. The Zod schema is the single source of truth for the input type — derive the TS type with `z.infer`, don't declare a parallel interface.
- Trim required strings: `z.string().trim().min(1, 'Name is required')`.
- Optional emails: `z.string().email('Must be a valid email').or(z.literal(''))`.
- Export a `sortSlugs` const map (`as const satisfies Record<string, EntitySort>`) from lowercase URL slug → backend column name. Slugs are short and human-readable (e.g. `contact: 'ContactName'`), hand-mapped per entity. Derive `SortSlug` as `keyof typeof sortSlugs`.
- `searchSchema` has three optional fields: `search` (string), `sort` (z.string().regex matching `^(slug1|slug2|...)-?$` — single param where the trailing `-` means descending), `page` (z.coerce.number().int().positive()). `page` must use `z.coerce.number()` because URL search params arrive as strings.
- URL shape: `?sort=name` (asc), `?sort=name-` (desc), omitted (unsorted — BE picks its own default).

## `<entity>.functions.ts` rules

Five exports, all `createServerFn` with `.inputValidator(<zod schema>)`. Never pass a typed identity function.

- `fetchEntities` — `method: 'GET'`, inputValidator `searchSchema`, handler translates the URL `sort` slug to the BE `{ sort, sortDirection }` shape via `sortSlugs` before awaiting `getEntities(...)`. When `sort` is absent, omit both fields.
- `fetchEntityById` — `method: 'GET'`, inputValidator `z.string().uuid()`, handler **swallows 404 → null**: `try { return await getEntityById(id); } catch (error) { if (error instanceof ApiRequestError && error.status === 404) return null; throw error; }`. This lets `$id.tsx` render an empty state instead of crashing.
- `createEntityFn` — `method: 'POST'`, inputValidator `entitySchema`, handler awaits `createEntity(data)`.
- `updateEntityFn` — `method: 'POST'`, inputValidator `z.object({ id: z.string().uuid(), data: entitySchema })`, handler destructures `{ id, data }` and awaits `updateEntity(id, data)`.
- `archiveEntityFn` — `method: 'POST'`, inputValidator `z.string().uuid()`, handler awaits `archiveEntity(id)`. No return value.

All mutating fns use `method: 'POST'` regardless of the underlying HTTP verb — `method` here only configures the server-fn RPC channel.

## Adding or removing fields

1. Add/remove on the BE `Entity`, `EntityRequest`, and migration.
2. From `packages/web/`: `bun run gen:api` to regenerate `src/api/schema.ts`, then `bun run check:fix`.
3. Update `entitySchema` in `<entity>.schemas.ts`.
4. Form + list pick up the new type automatically.

Canonical example: `packages/web/src/features/customers/customers.schemas.ts`, `customers.server.ts`, and `customers.functions.ts`.
