---
name: fe-data-access
description: Invoke before writing or modifying `-schemas.ts` (Zod entity + search schemas, sortSlugs) or `-server.ts` (TanStack server functions wrapping openapi-fetch) for an admin entity.
user-invocable: false
---

Consumed by [[fe-form]] (validators) and [[fe-list]] (loader + mutations). Mirrors backend contracts from [[be-crud-endpoints]].

## Files

Two route-private files under `src/routes/admin/<entities>/`:

- `-schemas.ts` — Zod entity schema, `sortSlugs` map, `searchSchema`. Source of truth for input types.
- `-server.ts` — five `createServerFn` exports wrapping the openapi-fetch client functions in `src/api/<entities>/`.

The dash prefix excludes these files from TanStack Router file routing. Imports keep the dash: `from './-server'`.

## `-schemas.ts` rules

- `entitySchema` is a `z.object({...})`. The Zod schema is the single source of truth for the input type — derive the TS type with `z.infer`, don't declare a parallel interface.
- Trim required strings: `z.string().trim().min(1, 'Name is required')`.
- Optional emails: `z.string().email('Must be a valid email').or(z.literal(''))`.
- Export a `sortSlugs` const map (`as const satisfies Record<string, EntitySort>`) from lowercase URL slug → backend column name. Slugs are short and human-readable (e.g. `contact: 'ContactName'`), hand-mapped per entity. Derive `SortSlug` as `keyof typeof sortSlugs`.
- `searchSchema` has three optional fields: `search` (string), `sort` (z.string().regex matching `^(slug1|slug2|...)-?$` — single param where the trailing `-` means descending), `page` (z.coerce.number().int().positive()). `page` must use `z.coerce.number()` because URL search params arrive as strings.
- URL shape: `?sort=name` (asc), `?sort=name-` (desc), omitted (unsorted — BE picks its own default).

## `-server.ts` rules

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
3. Update `entitySchema` in `-schemas.ts`.
4. Form + list pick up the new type automatically.

Canonical example: `packages/web/src/routes/admin/customers/-schemas.ts` and `-server.ts`.
