---
name: git-commit
description: Commit open changes using the Conventional Commits specification
user-invocable: true
model: haiku
allowed-tools: Bash(git add:*), Bash(git status:*), Bash(git diff:*), Bash(git log:*), Bash(git commit:*), Bash(git ls-files:*), Bash(git rev-parse:*), Bash(xargs:*), Bash(date:*), Read, Write, Edit
---

## Context

- Current git status: !`git status`
- Current git diff (tracked + untracked, via intent-to-add): !`git ls-files --others --exclude-standard -z | xargs -0 git add --intent-to-add 2>/dev/null; git diff HEAD`
- Current branch: !`git branch --show-current`
- Recent commits: !`git log --oneline -10`
- Today's date: !`date +%Y-%m-%d`
- Repo root: !`git rev-parse --show-toplevel`

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

Stage relevant files — including untracked files that belong with the change (e.g. new source files, their colocated tests, new fixtures) — and create the commit in a single message. Note: the diff above marks untracked files as intent-to-add so their content is visible, but they still need an explicit `git add <path>` to be committed. Do not use any other tools or do anything else.

### Update CHANGELOG.md (before committing)

Maintain a `CHANGELOG.md` at the **repo root** (the path from `git rev-parse --show-toplevel` above). It tracks the **first line** of each commit message, grouped by day in reverse-chronological order.

Steps:

1. After deciding the commit message, determine its first line (the `<type>(<scope>): <description>` line).
2. Read `CHANGELOG.md` at the repo root. If it does not exist, create it with this initial content:

   ```markdown
   # Changelog

   ## <today's date — YYYY-MM-DD>

   - <commit message first line>
   ```

3. If it exists:
   - If a `## <today's date>` section already exists, append `- <commit message first line>` to the bottom of that section's bullet list.
   - Otherwise, insert a new `## <today's date>` section directly below the `# Changelog` heading (above any existing day sections), containing the single bullet.

4. Stage `CHANGELOG.md` together with the other files so it lands in the same commit.

## Output

After committing, confirm with: "Done, committed with message: `<message>`"
