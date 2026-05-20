# Customers Hi-Fi Redesign

Redesign the customers admin UI from a flat table to a master/detail split-pane layout matching the hi-fi design. Left panel: scrollable customer list with filter tabs (All/Active/Archived) and inline search. Right panel: read-only detail view with Edit/Archive/Unarchive actions.

No new model fields. No contracts data. Uses only existing `CustomerResponse` fields.

## Backend

Add optional `includeArchived` query parameter to `GET /api/customers` (`bool?`, default `false` = active only, `true` = include archived). Required so the left panel can load all customers in one call.

Affected: `ICustomerRepository`, `CustomerRepository`, `CustomerService`, `CustomerEndpoints`.

After endpoint changes, regenerate `packages/web/src/api/schema.ts` via `bun run gen:api` (manual step — requires backend running at localhost:5204).

## Chapter 5 — FE data access

- `customers.schemas.ts`: add `filter` to `searchSchema` (`z.enum(["all", "active", "archived"])`, optional)
- `customers.server.ts`: add `includeArchived?: boolean` to `ListCustomersParams`; update `getCustomers` call
- `customers.functions.ts`: add `fetchAllCustomers` server fn (loads all customers, `includeArchived: true`, `pageSize: 9999`); add `unarchiveCustomerFn`

## Chapter 6 — FE UI

### Routing changes
- Move current `$id.tsx` edit form → `$id/edit.tsx` (new file, same content)
- New `$id.tsx`: read-only detail view in master/detail layout
  - Loader: calls `fetchAllCustomers` (left panel) + `fetchCustomerById` (detail)
  - `validateSearch`: same extended `searchSchema` (search + filter tabs persist in URL)
- Updated `index.tsx`: master/detail layout with empty right panel
  - Loader: calls `fetchAllCustomers`
  - Removes pagination, SortableHeader, TablePagination

### Left panel (`-components/customer-list-panel.tsx`)
Props: `customers: Customer[]`, `selectedId?: string`, `search?: string`, `filter?: "all"|"active"|"archived"`

- Search input: debounced, updates URL `search` param, filters client-side by name or contactName
- Filter tabs (shadcn Tabs): All / Active / Archived — updates URL `filter` param
- Scrollable customer rows (no pagination):
  - Initials chip (first 2 chars of name, uppercase, small circle)
  - Name + `#XXXXXX · City` subline
  - Archived badge (shadcn Badge, variant="secondary") when `isArchived`
  - Selected state: green left border rail (`border-l-2 border-[#00FF00]`) + tinted background
  - Click navigates to `/admin/customers/$id` preserving search/filter params
- Footer: "N of N customers" count

### Right panel — detail (`-components/customer-detail-panel.tsx`)
Props: `customer: Customer`, `onArchiveSuccess: () => void`

- Header row:
  - 72px initials avatar (first 2 chars, uppercase, dark bg circle)
  - Eyebrow: `#XXXXXX` (zero-padded 6-digit number)
  - 36px name heading (`text-3xl font-bold`)
  - Status pill: Active (green outline) or Archived (muted)
  - City pill
- Action buttons: Edit (link to `/$id/edit`), Archive or Unarchive (AlertDialog, calls `archiveCustomerFn`/`unarchiveCustomerFn`)
- Read-only field grid (3 columns, CSS grid):
  - Name (col 1), Number (col 2), blank (col 3)
  - Contact name (col 1), Email (col 2–3, spans 2)
  - Street (col 1–2, spans 2), blank (col 3)
  - Zip (col 1), City (col 2), Country (col 3)

### Right panel — empty (`-components/customer-empty-panel.tsx`)
Simple centered placeholder: "Select a customer to view details"

## Acceptance criteria

1. `/admin/customers` shows master/detail with left panel and placeholder right panel
2. Clicking a customer navigates to `/admin/customers/<id>`, highlights the row, shows detail
3. Active tab shows only non-archived; Archived tab shows only archived; All shows both
4. Search input filters the left panel by name or contact name (client-side)
5. Edit button navigates to `/admin/customers/<id>/edit` with the existing form
6. Archive/Unarchive button shows AlertDialog and refreshes the list after success
7. `bun run check` passes in `packages/web`
