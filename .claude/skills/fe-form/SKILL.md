---
name: fe-form
description: Invoke before writing or modifying an admin entity's create/edit form (`-components/form.tsx`, `new.tsx`, `$id.tsx`). TanStack Form + Zod + Card + Sonner toast.
user-invocable: false
---

Depends on the schema + server functions from [[fe-data-access]].

## Files

- `-components/form.tsx` — the form component, used by both create and edit routes.
- `new.tsx` — renders the form with empty defaults + the create server fn.
- `$id.tsx` — loader for `fetchEntityById`; renders the form with loaded values + the update server fn.

## Form component rules

Props: `{ initial: Partial<EntityInput>; onSubmit: (values: EntityInput) => Promise<unknown>; title: string }`. The component is reused for create and edit; only the parent route differs.

- `useForm` with `defaultValues` listing every field explicitly (`initial.field ?? ''`), pinned to the type via `satisfies EntityInput`. Coverage matters — a missing default silently becomes `undefined` at runtime.
- `validators: { onChange: entitySchema }` — pass the Zod schema directly. Never an inline ad-hoc function.
- `onSubmit` → `try { await onSubmit(value); toast.success('Entity saved'); router.navigate({ to: '/admin/entities' }); } catch { toast.error('Failed to save entity'); }`.
- Outer wrapper is shadcn `<Card>` with `<CardHeader><CardTitle>{title}</CardTitle></CardHeader>` and `<CardContent>` holding the `<form>`. Never a bare `<div>`.
- `<form className="grid gap-4">` with `onSubmit` that calls `preventDefault()` + `stopPropagation()` + `form.handleSubmit()`. Event param: `submitEvent` (descriptive name; see project naming convention).
- Render each field with `<form.Field name="...">{(field) => <TextField field={field} label="..." />}</form.Field>`. `TextField` (`#/components/text-field`) renders `<Label>` + shadcn `<Input>` + `<FieldError />` together — don't inline `field.state.meta.errors`.
- For non-text inputs use the matching shadcn primitive (`Select`, `Checkbox`, `RadioGroup`, `Textarea`) wrapped with the same `field` plumbing and an explicit `<FieldError field={field} />`.
- Side-by-side fields: wrap the pair in `<div className="grid grid-cols-2 gap-4">`.
- Autofocus the first field with `autoFocus`.
- Submit row uses `form.Subscribe` with selector `(state) => [state.canSubmit, state.isSubmitting] as const` and renders Save + Cancel buttons.
- Save button: `disabled={!canSubmit}`; label `'Saving…'` (with `…`) while submitting, `'Save'` otherwise.
- Cancel button: `type="button"`, `variant="outline"`, navigates back to the list, `disabled={isSubmitting}`.

## `new.tsx`

`createFileRoute('/admin/entities/new')` with just `component`. Renders `<EntityForm title="New entity" initial={{}} onSubmit={(value) => createEntityFn({ data: value })} />`.

## `$id.tsx`

`createFileRoute('/admin/entities/$id')` with `loader: ({ params }) => fetchEntityById({ data: params.id })`. Component reads `Route.useLoaderData()` and `Route.useParams()`. If the loaded entity is `null`, render `<p>Entity not found.</p>`. Otherwise render `<EntityForm title={`Edit ${entity.name}`} initial={entity} onSubmit={(value) => updateEntityFn({ data: { id, data: value } })} />`.

The form never knows whether it's creating or editing — the parent route chooses the server fn.

Canonical example: `packages/web/src/routes/admin/customers/-components/form.tsx`, `new.tsx`, `$id.tsx`.
