---
name: crud-entity
description: Invoke this before writing any CRUD entity code — whether via piv-implement or directly. Canonical patterns for Create/List/Read/Update/Archive with paging, sorting, and filtering.
user-invocable: false
---

## Backend

- Operations: `POST /` (create), `GET /` (list), `GET /{id}` (read), `PUT /{id}` (update), `PATCH /{id}/archive` + `PATCH /{id}/unarchive` (soft delete).
- PK is `Guid Id`.
- Delete is soft via `bool IsArchived`. No hard DELETE.
- List endpoint accepts `search`, `sort`, `sortDirection`, `page`, `pageSize` as query params. Filter excludes archived rows by default.
- Optional API query params must be nullable (`int? page`, `Sort? sort`) — Minimal API treats non-nullable as required.
- Sort enum exposes the allowed sortable columns. `SortDirection` enum is `Asc | Desc`.
- Enums serialize as PascalCase strings (global `JsonStringEnumConverter`).

```csharp
// Entity
public class Widget
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public bool IsArchived { get; set; }
}

// Sort enums
public enum WidgetSort { Name }
public enum SortDirection { Asc, Desc }

// Endpoints
app.MapGet("/widgets", (string? search, WidgetSort? sort, SortDirection? dir, int? page, int? pageSize, WidgetService svc)
    => svc.List(search, sort, dir, page, pageSize));

app.MapPatch("/widgets/{id}/archive",   (Guid id, WidgetService svc) => svc.SetArchived(id, true));
app.MapPatch("/widgets/{id}/unarchive", (Guid id, WidgetService svc) => svc.SetArchived(id, false));
```

## Frontend

- Route files live under `src/routes/admin/<entity>/`. Files/dirs prefixed with `-` are colocated and excluded from routing.
- `-schemas.ts` defines the Zod entity schema and `searchSchema` (search/sort/sortDirection/page). Share the schema between form and server function.
- `-server.ts` exports TanStack Start server functions: `fetchWidgets`, `fetchWidgetById`, `createWidgetFn`, `updateWidgetFn`, `archiveWidgetFn`. Each wraps the openapi-fetch client and passes a Zod schema to `.inputValidator`.
- `index.tsx` — list route: `validateSearch: searchSchema`, loader calls `fetchWidgets`. Debounced search input, `SortableHeader` per sortable column, `TablePagination`, archive via `AlertDialog` + Sonner toast.
- `new.tsx` / `$id.tsx` — create/edit routes: render `<WidgetForm>` with empty defaults or loader data.
- `-components/widget-form.tsx` — Card-wrapped TanStack Form, `validators: { onChange: widgetSchema }`, `TextField` per field, Sonner toast on success, navigate back to list on save.

```tsx
// -schemas.ts
export const widgetSchema = z.object({ name: z.string().trim().min(1, 'Name is required') });
export type WidgetInput = z.infer<typeof widgetSchema>;

export const sortColumns = ['Name'] as const;
export type SortColumn = (typeof sortColumns)[number];

export const searchSchema = z.object({
  search: z.string().optional(),
  sort: z.enum(sortColumns).optional(),
  sortDirection: z.enum(['Asc', 'Desc']).optional(),
  page: z.coerce.number().int().positive().optional(),
});

// -server.ts
export const fetchWidgets = createServerFn({ method: 'GET' })
  .inputValidator(searchSchema)
  .handler(async ({ data }) => getWidgets(data));

export const fetchWidgetById = createServerFn({ method: 'GET' })
  .inputValidator(z.string().uuid())
  .handler(async ({ data: id }) => {
    try { return await getWidgetById(id); }
    catch (error) {
      if (error instanceof ApiRequestError && error.status === 404) return null;
      throw error;
    }
  });

export const createWidgetFn = createServerFn({ method: 'POST' })
  .inputValidator(widgetSchema)
  .handler(async ({ data }) => createWidget(data));

export const updateWidgetFn = createServerFn({ method: 'POST' })
  .inputValidator(z.object({ id: z.string().uuid(), data: widgetSchema }))
  .handler(async ({ data: { id, data } }) => updateWidget(id, data));

export const archiveWidgetFn = createServerFn({ method: 'POST' })
  .inputValidator(z.string().uuid())
  .handler(async ({ data: id }) => archiveWidget(id));

// index.tsx (list route)
export const Route = createFileRoute('/admin/widgets/')({
  validateSearch: searchSchema,
  loaderDeps: ({ search }) => ({
    search: search.search,
    sort: search.sort,
    sortDirection: search.sortDirection,
    page: search.page,
  }),
  loader: ({ deps }) => fetchWidgets({ data: deps }),
  component: WidgetList,
});

// -components/widget-form.tsx
const form = useForm({
  defaultValues: { name: initial.name ?? '' } satisfies WidgetInput,
  validators: { onChange: widgetSchema },
  onSubmit: async ({ value }) => {
    try {
      await onSubmit(value);
      toast.success('Widget saved');
      router.navigate({ to: '/admin/widgets' });
    } catch {
      toast.error('Failed to save widget');
    }
  },
});
```
