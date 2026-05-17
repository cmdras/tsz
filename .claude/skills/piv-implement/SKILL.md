---
name: piv-implement
description: Strict chapter-based plan executor. User invokes via `/piv-implement [path-to-PLAN.md] [--chapter N]`. Implements PLAN.md one chapter at a time (tests first, then backend, then frontend), runs the per-chapter test gate, and appends a terse chapter log to IMPL.md. Step 2 of the manual Plan → Implement → Validate workflow.
model: sonnet
---

You are running `/piv-implement` — step 2 of the PIV workflow. Execute PLAN.md **one chapter per invocation**. Stop at the end of the chapter so the user can review, `/compact`, or course-correct before the next pass. **Do not commit. Do not deviate. Do not chain chapters.**

## Arguments

- Optional path to PLAN.md (default `./PLAN.md`).
- Optional `--chapter N` to force a chapter; otherwise infer from IMPL.md. Re-running a completed chapter is allowed — the existing section in IMPL.md is overwritten in place.

If PLAN.md doesn't resolve: one-line error, stop. The slot is PLAN.md's directory; IMPL.md is written next to it. Print `Slot: <dir>/` and `Chapter <N>: <name>` before anything else.

## Pre-flight

1. Read PLAN.md. If structurally broken: report, stop, do not repair.
2. Read IMPL.md if it exists. The highest `done` chapter determines the next one (unless `--chapter` overrides).
3. No IMPL.md and no `--chapter` flag: start at chapter 1.

## Chapter map

1. **Tests** — integration tests for the CRUD flows + unit tests for the business rules in PLAN.md. Tests must compile and **fail** (no implementation exists yet). No production code in this chapter.
2. **Schema + EF** — entity, DbContext config, migration.
3. **Service** — business logic; goal is unit tests green.
4. **Endpoints** — Minimal API surface; goal is integration tests green.
5. **FE data access** — `<entity>.schemas.ts`, `.server.ts`, `.functions.ts`.
6. **FE UI** — list page + form page(s) + form component.

If PLAN.md is backend-only or frontend-only, skip chapters that don't apply and record the skip in IMPL.md.

## Chapter gate

After implementing the chapter, run typecheck and tests on touched files (see Auto-detect):

- **C1 (Tests):** tests compile; every new test fails (or errors with a clear "not implemented" reason). A new test that accidentally passes is a gate failure — the test isn't real.
- **C2–C4 (Backend):** strictly more tests passing than before this chapter; zero previously-green tests went red.
- **C5–C6 (Frontend):** typecheck clean on touched files; zero test regressions.

If the gate fails: append the chapter log with status `blocked`, surface the issue, stop. Do not patch up.

## Discipline

- **In scope only.** Out-of-scope refactors, nearby bugs, typos — leave alone.
- **One chapter per invocation.** Never start the next chapter in the same run.
- **Pause on assumption failure.** If PLAN.md's premise is wrong, stop and surface — do not invent a workaround.
- **Never commit.** No `git commit`, `git add`, `git push`.

## IMPL.md — a log, not a description

IMPL.md records what the code can't say. Never describe content that lives in the repo: no file-changed lists (the diff has them), no method enumerations (read the class), no test name lists (read the test file), no architecture restatements (PLAN.md has them).

Create IMPL.md on chapter 1; append on every subsequent chapter. Re-running a chapter via `--chapter N` overwrites that section in place; later sections (if any) remain untouched. Structure:

- Header: feature title, link to PLAN.md, started timestamp, current cursor (e.g. `Chapter: 3/6`).
- One `## Chapter N: <name> — <done|blocked|skipped>` section per completed chapter, in order. Each section is at most ~4 bullets:
  - **Tests:** `<before> failing → <after> failing, <regressions> regressions` (omit for frontend chapters if no tests apply).
  - **Deviation:** one line, only if behavior differs from PLAN.md, with the reason. Omit if none.
  - **Manual:** commands or steps the user must run themselves (migrations, seeds, env vars). Omit if none.
  - **Carry-forward:** known gaps left for a future slice. Omit if none.
- Final `## Acceptance check` section appears only after the last chapter — bullets for PLAN.md acceptance criteria with ✓/✗ and a one-line note each.

Rule of thumb: if a bullet could be replaced by "run `git diff`" or "read `<file>`", delete it.

## Final pass (last chapter only)

1. Run the full typecheck/test suite (whole project, not just touched files).
2. Write the Acceptance check section.
3. Print a one-line summary: `Chapter 6/6 done — <pass|fail> acceptance.` Do not chain into `/piv-validate`.

## Auto-detect typecheck/tests

Walk up from the slot and inspect the project to discover how it runs typecheck and tests (and lint if available). Use manifests, lockfiles, Makefile/Justfile targets, CI configs, README, contributor docs. Don't assume a specific toolchain.

If a tool is configured but not installed: `skipped: <tool> not installed`. Do not install tooling.
