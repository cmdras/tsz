# Web Package

## UI Components

Use [shadcn/ui](https://ui.shadcn.com/) for UI. Don't reach for raw HTML elements or other component libraries when a shadcn component covers the case. Add new components via `bunx shadcn@latest add <component>` from this package directory, then run `bun run check:fix`.

## Forms

Forms use [`@tanstack/react-form`](https://tanstack.com/form). Validate every form with Zod via the form's validator adapter — never inline ad-hoc `validators.onChange` functions.

Render each field as `<Label>` + `<Input>` (or shadcn equivalent) + `<FieldError field={field} />`. Don't inline `field.state.meta.errors` rendering in the JSX — it duplicates and drifts across fields. `FieldError` lives in `#/components/field-error.tsx` and only renders once the field is touched.

## Server functions

Every `createServerFn` must pass a Zod schema to `.inputValidator`, not a typed identity function. The schema is the source of truth for the input type — don't separately declare a TS type alongside it.

Share schemas between the form and its server function: define the Zod schema once (typically next to the server function in `src/api/<resource>/`) and import it into the route. This keeps client and server validation aligned.

## Route files

Files and directories prefixed with `-` are excluded from TanStack Router's file routing — use them to colocate route-private schemas, server functions, and components alongside the route that owns them (e.g. `-schemas.ts`, `-server.ts`, `-components/`). Imports include the dash: `from './-server'`.

## API schema

Regenerate the OpenAPI-typed client (`src/api/schema.ts`) via `bun run gen:api`, then run `bun run check:fix`.

## Testing

- Skip creating tests for components
