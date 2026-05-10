---
name: git-commit
description: Commit open changes using the Conventional Commits specification
user-invocable: true
model: haiku
allowed-tools: Bash(git add:*), Bash(git status:*), Bash(git diff:*), Bash(git log:*), Bash(git commit:*)
---

## Context

- Current git status: !`git status`
- Current git diff (staged and unstaged changes): !`git diff HEAD`
- Current branch: !`git branch --show-current`
- Recent commits: !`git log --oneline -10`

## Your task

Based on the above changes, create a single git commit following the **Conventional Commits** specification.

### Commit message format

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

### Type — pick exactly one

| Type | When to use |
|------|-------------|
| `feat` | A new feature (bumps MINOR in SemVer) |
| `fix` | A bug fix (bumps PATCH in SemVer) |
| `docs` | Documentation only changes |
| `style` | Formatting, whitespace — no logic change |
| `refactor` | Code restructuring with no feature or bug change |
| `perf` | Performance improvement |
| `test` | Adding or correcting tests |
| `build` | Build system or dependency changes |
| `ci` | CI/CD configuration changes |
| `chore` | Maintenance tasks that don't fit above |

### Scope (optional)

A noun in parentheses naming the affected area, e.g. `feat(auth):` or `fix(parser):`.
Derive the scope from the files changed (module, package, component name).
Omit scope when the change is truly cross-cutting.

### Description rules

- Lowercase, imperative mood ("add" not "added" or "adds")
- No period at the end
- 72 characters or fewer

### Body (optional)

Add a body when the **why** behind the change is non-obvious. Separate from the description with one blank line. Wrap at 72 characters.

### Breaking changes

Indicate a breaking change in one or both of these ways:
1. Append `!` after the type/scope: `feat(api)!: remove deprecated endpoint`
2. Add a `BREAKING CHANGE: <description>` footer (one blank line after the body)

`BREAKING CHANGE` must be uppercase. This bumps MAJOR in SemVer.

### Footer tokens (optional)

Reference issues or add metadata after the body:
```
Reviewed-by: Alice
Refs: #123
BREAKING CHANGE: `env` option removed
```

### Decision process

1. Look at the diff to identify the primary change category.
2. If multiple types apply, choose the most significant (feat > fix > refactor > chore).
3. Infer the scope from the changed files/modules.
4. Check recent commits to match the project's scope conventions.
5. If any public API, interface, or CLI contract changes in an incompatible way, mark it as a breaking change.
6. Avoid committing files that likely contain secrets (.env, credentials.json, etc.).

Stage relevant files and create the commit in a single message. Do not use any other tools or do anything else.

## Output

After committing, confirm with: "Done, committed with message: `<message>`"
