---
name: fe-list
description: Invoke before writing or modifying an admin entity's list route (`index.tsx`). validateSearch, debounced search, SortableHeader, TablePagination, archive AlertDialog + router.invalidate.
user-invocable: false
---

Depends on the schema + server functions from [[fe-data-access]].

## File

`src/routes/admin/<entities>/index.tsx` — search input, sortable table, archive confirmation, pagination.

## Route options

- `validateSearch: searchSchema` — parses `?search=&sort=&page=`. Sort uses a single param: `?sort=name` (asc), `?sort=name-` (desc), omitted (unsorted).
- `loaderDeps: ({ search }) => ({ search, sort, page })` — MUST list every search param the loader reads, otherwise sort/page changes won't refetch.
- `loader: ({ deps }) => fetchEntities({ data: deps })`.
- `component`.

Top-level constant: `const PAGE_SIZE = 25;` (mirrors BE default).

## Component reads

- `const { items, total } = Route.useLoaderData();`
- `const { search, sort, page = 1 } = Route.useSearch();` — no defaults for `sort`; absent = unsorted (BE applies its own fallback).
- `const router = useRouter();` · `const navigate = Route.useNavigate();`
- `const totalPages = Math.max(1, Math.ceil(total / PAGE_SIZE));`

## Handlers

- **Search** — `useDebouncedCallback((value: string) => navigate({ search: prev => ({ ...prev, search: value || undefined, page: undefined }) }), 300)`. Hook lives at `#/hooks/use-debounced-callback`.
- **Sort toggle** — `(column: string) => navigate({ search: prev => ({ ...prev, sort: prev.sort === column ? \`${column}-\` : column, page: undefined }) })`. 2-state cycle: clicking a column sets asc, clicking again flips to desc, clicking another resets to asc on the new column.
- **Page** — `(targetPage: number) => navigate({ search: prev => ({ ...prev, page: targetPage === 1 ? undefined : targetPage }) })`.
- **Archive** — `async (id) => { try { await archiveEntityFn({ data: id }); toast.success('Entity archived'); router.invalidate(); } catch { toast.error('Failed to archive entity'); } }`.

## Navigation rules

- Always use the updater form `(previousSearch) => ({ ...previousSearch, ... })` so unrelated params survive.
- Any param that changes the result set (search, sort) resets `page: undefined`.
- Empty/default values are `undefined`, never `''` or `1` — keeps URLs clean.
- Call `router.invalidate()` after every mutation to re-run the loader.

## Render rules

- Header row: `<h1 className="text-2xl font-bold">` + a `<Button asChild><Link to="/admin/entities/new">New entity</Link></Button>`, wrapped in `flex items-center justify-between mb-4`.
- Search row: shadcn `<Input>` with `defaultValue={search ?? ''}`, `onChange={(changeEvent) => handleSearch(changeEvent.target.value)}`, `className="max-w-xs"`. Event param: `changeEvent` (descriptive name).
- `<Table>` (shadcn) with `<SortableHeader column="<slug>" label="X" active={sort} onToggle={toggleSort} />` for each sortable column (use the lowercase slug from `sortSlugs`), plain `<TableHead>` for non-sortable, and an empty `<TableHead />` for the actions column.
- Each row's primary cell wraps a `<Link to="/admin/entities/$id" params={{ id: entity.id }} className="hover:underline">` — do NOT make the whole row clickable.
- Actions cell (`className="text-right"`) holds an `<AlertDialog>` with `<AlertDialogTrigger asChild><Button size="sm" variant="outline">Archive</Button></AlertDialogTrigger>` and the standard `Header/Title/Description/Footer/Cancel/Action` body. `AlertDialogAction onClick={() => handleArchive(entity.id)}`.
- Empty state: a single `<TableRow><TableCell colSpan={N} className="text-center text-muted-foreground">No entities found.</TableCell></TableRow>` where `N` matches column count.
- `<TablePagination page={page} totalPages={totalPages} total={total} onChange={goToPage} />` at the bottom.

Canonical example: `packages/web/src/routes/admin/customers/index.tsx`.
