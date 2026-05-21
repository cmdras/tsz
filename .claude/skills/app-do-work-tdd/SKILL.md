---
name: do-work-tdd
description: Execute a unit of work end-to-end: plan, implement with tdd, validate with typecheck and tests, then commit. Use when user wants to do work, build a feature, fix a bug, or implement a phase from a plan.
argument-hint: '[issue-file] — path to an issue markdown file.'
disable-model-invocation: true
---

# Do Work (TDD)

Execute a complete unit of work: plan it, build it, validate it, commit it.

## Workflow

### 1. Understand the task

If an issue file was passed as an argument, read it first — it is the source of truth for scope, acceptance criteria, and any references. Otherwise, abort the skill and ask the user to provide an issue file.

Then explore the codebase to understand the relevant files, patterns, and conventions. Delegate codebase exploration beyond ~3 greps to the built-in `Explore` agent to keep context light.

If the task is ambiguous, ask the user to clarify scope before proceeding.

### 1.5 Switch Branch(Optional)

Ensure that the to be implemented work is in a feature branch. Create and switch if the current branch is master. Skip if we are already in a feature branch.

### 2. Implement

Work through the plan step by step.

**Backend code** (`packages/api/**`): invoke `Skill('tdd')` and follow its red/green/refactor loop, one vertical slice at a time.

**Frontend code** (`packages/web/**`): implement directly without TDD.

### 3. Validate

Run the feedback loops and fix any issues. Repeat until all pass cleanly.

```bash
bun run check     # static analysis of Typescript code with linting, typechecking, and formatting
bun run test:web  # runs frontend unit tests
bun run test:api  # runs backend unit tests
```

### 4. Code review

Run `Skill('code-review')` to simplify the code.

Run the validation loops again and fix any issues. Repeat until all pass cleanly.

```bash
bun run check     # static analysis of Typescript code with linting, typechecking, and formatting
bun run test:web  # runs frontend unit tests
bun run test:api  # runs backend unit tests
bun run test:api:int # runs integration tests
```

### 5. Commit

Once static analysis and tests pass

- commit the work. Run `Skill('git-commit')` to commit the work.
- push the feature branch and create a PR. Link the current issue to the PR

### 6. Report QA

Write a list of items the user should test to verify the work.
