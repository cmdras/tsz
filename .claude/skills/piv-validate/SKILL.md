---
name: piv-validate
description: Runs the project's auto-detected linter/formatter/tests whole-project and writes a slim VALIDATION.md verdict. User invokes via `/piv-validate` from a slot folder containing PLAN.md and IMPL.md. Step 3 of the manual Plan → Implement → Validate workflow.
model: haiku
---

You are running the `/piv-validate` skill — step 3 of the PIV workflow. Run the project's auto-detected lint/format/test commands whole-project and write VALIDATION.md.

No external reviewers. No code review. The user handles review.

## Arguments

None. Runs in the current working directory, which **must** be the slot folder.

## Phase 0 — Pre-flight

Abort on the first failure with a one-line error:

1. `./PLAN.md` exists and is readable.
2. `./IMPL.md` exists and is readable.
3. cwd is inside a git repository (`git rev-parse --is-inside-work-tree`).

Print `Slot: $(pwd)`.

## Phase 1 — Auto-detect and run checks

Walk up from cwd and inspect the project to discover how it runs **lint**, **format**, **test**, and **typecheck** whole-project. Use whatever the project actually uses — manifests, lockfiles, Makefile/Justfile targets, CI configs, README, contributor docs. Don't assume a specific toolchain; let the project tell you.

Run each discovered check whole-project, capturing combined stdout+stderr and exit code. If a check isn't configured for this project, omit it. If none are discoverable, write `auto-checks: none detected` in VALIDATION.md and proceed.

If a tool is configured but not installed, treat as `skipped: <tool> not installed` — not a failure.

## Phase 2 — Write VALIDATION.md

Overwrite any prior file at `./VALIDATION.md`. Structure:

```
# VALIDATION

## Verdict
**<pass | fail>** — <1 sentence reasoning>

## Checks
- <command>: exit <N>
- <command>: exit <N>

## Failure output
<only present if any check failed; per failed check: command followed by key failure lines>
```

Verdict: **pass** if every check exited 0 (skipped tools don't count as failures). **fail** otherwise.

Print the verdict line and the path to VALIDATION.md. Stop. Do not chain.
