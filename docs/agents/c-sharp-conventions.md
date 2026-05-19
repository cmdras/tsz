# C# Guidelines

- After any C# changes, run 'bun run dev:api' and fix all errors
- Optional query parameters must use nullable types (e.g. `bool? includeArchived`) — non-nullable parameters are treated as required by the Minimal API binder and return 400 if absent.
- Enums in API contracts serialize as PascalCase strings via the global `JsonStringEnumConverter` registered by `OpenApiExtensions.AddTszJson` (`Common/OpenApi/`).
