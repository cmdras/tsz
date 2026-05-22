This is a monorepo with a .NET backend, and Tanstack Start frontend.

- Use bun, not npm or pnmp to start the projects
- For C# changes, consult [docs/agents/c-sharp-conventions.md]
- For Typescript changes, consult [docs/agents/typescript-conventions.md]
- When reporting information to me, be extremely concise and sacrifice grammar for the sake of concision.
- Descriptive naming is a MUST: no acronyms, no single-letter variables, no abbreviated names. Full words only (e.g. `cancellationToken` not `ct`, `error` not `err`, `submitEvent` not `e`).
- Never manually edit generated code, always run it via the generation scripts.

## Agent skills

### Issue tracker

Issues live in GitHub Issues for `cmdras/tsz`. See `docs/agents/issue-tracker.md`.

### Triage labels

Default five-label vocabulary (no overrides). See `docs/agents/triage-labels.md`.

### Domain docs

Multi-context layout: `CONTEXT-MAP.md` at root, with `packages/api/CONTEXT.md` and `packages/web/CONTEXT.md`. See `docs/agents/domain.md`.
