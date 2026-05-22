# C# Guidelines

- After any C# changes, run 'bun run dev:api' and fix all errors
- Optional query parameters must use nullable types (e.g. `bool? includeArchived`) — non-nullable parameters are treated as required by the Minimal API binder and return 400 if absent.
- Enums in API contracts serialize as PascalCase strings via the global `JsonStringEnumConverter` registered by `OpenApiExtensions.AddTszJson` (`Common/OpenApi/`).
- When adding a new field to an existing `record` used across multiple call sites, add a new property rather than renaming an existing one — renames break every call site simultaneously and make the diff harder to review.
- Service and scheduler methods with multiple `IEnumerable<Guid>` (or similar collection) parameters should accept them as a named record/class, not positional arguments — positional signatures break all callers when a new collection parameter is inserted.
