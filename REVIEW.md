# REVIEW.md

## 08/05 - Peter

- Put your 'shadcn' remarks in /packages/web/CLAUDE.md, so a root CLAUDE.md for general stuff and a sub CLAUDE.md for specifics.
- Move duplicated tsx code to a component

```js
// bad
<div>
    <Input
        value={field.state.value}
        onChange={(e) => field.handleChange(e.target.value)}
        onBlur={field.handleBlur}
        autoFocus
    />
    {field.state.meta.errors.length > 0 && (
        <p className="mt-1 text-xs text-destructive">{field.state.meta.errors.join(', ')}</p>
    )}
</div>
```

```js
// impr
<div className="grid gap-2">
    <Label htmlFor={field.name}>Name</Label>
    <Input
    id={field.name}
    name={field.name}
    value={field.state.value}
    onBlur={field.handleBlur}
    onChange={(e) => field.handleChange(e.target.value)}
    />
    <FieldError field={field} />
</div>
```

en 

```js
function FieldError({ field }: { field: { state: { meta: { isTouched: boolean; errors: Array<unknown> } } } }) {
  if (!field.state.meta.isTouched || field.state.meta.errors.length === 0) return null;
  const message = field.state.meta.errors
    .map((err) => (typeof err === 'string' ? err : (err as { message?: string })?.message))
    .filter(Boolean)
    .join(', ');
  if (!message) return null;
  return <p className="text-sm text-destructive">{message}</p>;
}
```

- Add Zod validation to your server functions
- Add Zod validatiohn to you form
- Fix you typing issues

![](2026-05-08-10-46-37.png)
![](2026-05-08-10-48-07.png)