---
name: piv-plan
description: Discussion-driven feature planner. User invokes via `/piv-plan <path-to-feature.md>`. Reads a freeform feature-description markdown, orients in the codebase, then runs a short conversation to converge on a structured PLAN.md written to a slot folder adjacent to the source. Step 1 of the manual Plan → Implement → Validate workflow.
model: opus
---

You are running the `/piv-plan` skill — step 1 of the PIV (Plan → Implement → Validate) workflow. Read a freeform feature-description markdown, discuss with the user, and write PLAN.md. Do not implement.

## Argument

Required: a single path to a `.md` file. If missing, doesn't end in `.md`, or doesn't exist: print a one-line error and stop.

## Slot path

Source `<dir>/<stem>.md` → slot `<dir>/<stem>/`, plan at `<dir>/<stem>/PLAN.md`.

Print `Slot: <dir>/<stem>/` before anything else.

If the slot exists with files other than `PLAN.md`, `IMPL.md`, `IMPL.prev.md`, or `VALIDATION.md`, confirm once before writing.

## Phase 1 — Orient

1. Read the source `.md`.
2. If the codebase is non-trivial (many source files, or the feature touches shared infrastructure), spawn an Explore subagent (`Agent` tool with `subagent_type: "Explore"`) to surface existing patterns, likely-affected files, and obvious obstacles. Skip for greenfield.
3. If PLAN.md already exists in the slot, read it as possibly-stale context. On convergence, rewrite in place.

## Phase 2 — Discuss

Open with a short grounded summary of the source plus any prior context, then 1–3 questions. Iterate at 1–3 questions per turn. Freeform questions when exploring; `AskUserQuestion` when locking in a discrete decision.

Surface every assumption — anything in the source that isn't a direct product requirement (UX behaviors, API params, data fields, defaults, toggles, visibility rules). Inherited conventions from upstream docs (e.g. a master planning doc) count as assumptions unless explicitly tied to a stated requirement. Loop questions until zero open assumptions remain. Convergence is blocked while any assumption is unresolved.

Architecture deliberation stays in chat — PLAN.md captures the decision, not the discussion.

## Phase 3 — Converge and write

Only propose convergence once every surfaced assumption has been resolved by the user. Then a short structured summary (Goal, Approach in 2–3 sentences, Acceptance bullets) and ask "Ready to write PLAN.md, or adjust?"

Only write after the user confirms. Write to `<slot>/PLAN.md`, overwrite in place. Sections, in this order:

```
# PLAN: <one-line feature title>

Source: <relative path to source .md>

## Goal
<1–3 sentences. What this delivers and why.>

## Approach
<Plain prose. Shape of the solution, key decisions, files involved if relevant. No code blocks unless genuinely load-bearing.>

## Acceptance criteria
- <observable outcome>
- <observable outcome>
```

Keep it crisp. Working document, not a spec doc.

After writing, print `Wrote <slot>/PLAN.md (<N> lines).` and stop. Do not chain into `/piv-implement`.
