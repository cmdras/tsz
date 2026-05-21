# Domain Docs

How the engineering skills should consume this repo's domain documentation.

## Before exploring, read these

- **`CONTEXT-MAP.md`** at the repo root — it points at one `CONTEXT.md` per context. Read each one relevant to the topic.
- **`docs/adr/`** at the repo root — system-wide architectural decisions.
- Per-context ADRs:
  - `packages/api/docs/adr/` — backend decisions
  - `packages/web/docs/adr/` — frontend decisions

If any of these files don't exist, **proceed silently**. Don't flag their absence; don't suggest creating them upfront.

## File structure

```
/
├── CONTEXT-MAP.md
├── docs/adr/                         ← system-wide decisions
├── packages/
│   ├── api/
│   │   ├── CONTEXT.md                ← backend domain
│   │   └── docs/adr/
│   └── web/
│       ├── CONTEXT.md                ← frontend domain
│       └── docs/adr/
```

## Use the glossary's vocabulary

When your output names a domain concept, use the term as defined in the relevant `CONTEXT.md`. Don't drift to synonyms the glossary explicitly avoids.

If the concept isn't in any glossary yet, either you're inventing language the project doesn't use (reconsider) or there's a real gap (note it for `/grill-with-docs`).

## Flag ADR conflicts

If your output contradicts an existing ADR, surface it explicitly:

> _Contradicts ADR-0007 — but worth reopening because…_
