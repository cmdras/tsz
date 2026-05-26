---
name: app-changelog
description: Write a single user-facing CHANGELOG.md entry summarising what landed on the current feature branch. Use at the end of a feature branch before opening its PR.
---

# App Changelog

Write one dated section in `CHANGELOG.md` covering all the work on the current feature branch in user-facing terms.

## Workflow

1. **Find the work to describe** — run `git log --no-merges --format='%s%n%b' $(git merge-base master HEAD)..HEAD` to get the commits since branch divergence. Also `git diff --stat $(git merge-base master HEAD)..HEAD` for breadth context.

2. **Synthesize user-facing bullets** — each bullet answers "what can a user now do?" or "what behavior changed?" — not "what was built". No class/method names, no test counts, no migration names. Group related commits into single bullets. Aim for 2–8 bullets per feature branch.
   - ✓ "Admins can view all users and create new ones via an Add dialog"
   - ✓ "Creating a user automatically assigns a leave balance for each active leave type"
   - ✗ "Added UserService.CreateAsync with single SaveChangesAsync and 10 unit tests"

3. **Insert under today's date** — edit `CHANGELOG.md` at the repo root (`git rev-parse --show-toplevel`). If today's `## YYYY-MM-DD` section already exists, append bullets to it. Otherwise insert a new section directly below the `# Changelog` heading, above any existing day sections. Get today via `date +%Y-%m-%d`.

4. **Commit** — `git add CHANGELOG.md && git commit -m "docs: update changelog"`.
