---
name: app-orchestrate
description: Orchestrate solving every ready-for-agent GitHub issue end-to-end. Creates a feature branch, fans out each issue to a sonnet subagent, merges results into the feature branch, and opens a single PR to master for human QA.
argument-hint: '<feature-branch-name> — short kebab-case slug, prefixed with feature/'
disable-model-invocation: true
---

# App Orchestrate

Solve every `ready-for-agent` issue sequentially, accumulating commits on a single feature branch, then open ONE PR to master for human QA.

## Required argument

`<feature-branch-name>` — short kebab-case slug (e.g. `crap-cleanup`). The orchestrator creates and uses the branch `feature/<name>`.

If no argument is provided, abort and ask the user for one.

## Hard rules

- No GitHub PRs per issue. Each subagent commits onto a local sub-branch which the orchestrator merges into the feature branch with `git merge --no-ff`. Exactly ONE PR opens at the end: feature branch → master.
- CHANGELOG is written ONCE at the end via `Skill('app-changelog')`. Never per-issue.
- Issues are processed sequentially. No parallel subagents.
- Failed sub-branches are kept (never `git branch -D`) so the user can inspect.

## Workflow

### 1. Preflight and feature-branch setup

- Working tree must be clean (`git status --porcelain` empty). If not, abort with "commit or stash first".
- `feature/<name>` must NOT already exist locally or remotely. If it does, abort with "branch exists; resume not supported".
- `git checkout master && git pull --ff-only`.
- `git checkout -b feature/<name> && git push -u origin feature/<name>`.

### 2. Discover and order issues

- `gh issue list --repo cmdras/tsz --label ready-for-agent --state open --json number,title,body,labels,url --limit 200`.
- Parse each body's "Blocked by" section to build a DAG of issue dependencies (references like `#42` or full URLs).
- Topologically sort. If a cycle exists, abort and list the cycle.
- If the list is empty, abort and tell the user there's nothing to do.

### 3. Per-issue loop

Initialise `consecutive_failures = 0`. For each issue in topological order:

#### 3a. Cascade-skip

If any blocker carries `needs-triage` (original or applied during this run), skip. Remove `ready-for-agent`, apply `needs-triage`, comment "Skipped by /orchestrate: blocked by #N which failed." Move on (does NOT count toward consecutive_failures).

#### 3b. Prepare workspace

- `git checkout feature/<name> && git checkout -b issue-<N>-<slug>`. `<slug>` = first 4 kebab-cased words from the issue title.
- Ensure `.tmp/orchestrate/` exists. Write the issue body to `.tmp/orchestrate/issue-<N>.md` with a header line `# Issue #<N>: <title>` and the URL above the body.

#### 3c. Spawn the subagent

Call `Agent(subagent_type: "general-purpose", model: "sonnet", description: "Solve issue #<N>")`. The prompt must include:

- Absolute path to `.tmp/orchestrate/issue-<N>.md`, with instructions to invoke `Skill('app-do-work-tdd', '<path>')`.
- Current sub-branch name. Hard rules: do NOT switch branches, do NOT push, do NOT create a PR. All commits stay on the sub-branch.
- Output contract: the final message's LAST non-blank line must match `^(SUCCESS|FAILED: .+)$` exactly. Ambiguous scope → `FAILED: needs clarification: <question>`. Never ask the orchestrator clarifying questions.
- On `SUCCESS`, the subagent must list 2–6 QA bullets above the SUCCESS marker, one per behavior the user should manually verify.

#### 3d. Parse the subagent's response

Take the last non-blank line. Strict regex `^(SUCCESS|FAILED: .+)$`:

- `SUCCESS` → 3e.
- `FAILED: <reason>` → 3g.
- No match → treat as `FAILED: unparseable subagent output` → 3g.

#### 3e. Merge and validate

- `git checkout feature/<name>`
- `git merge --no-ff issue-<N>-<slug> -m "Merge issue #<N>: <title>"`
- Validation, sequentially (stop on first failure): `bun run check`, `bun run test:web`, `bun run test:api`, `bun run test:api:int`.
- Green → `git branch -d issue-<N>-<slug>`, remove `ready-for-agent`, apply `ready-for-qa`, save the subagent's QA bullets for the final PR body, `consecutive_failures = 0`, next issue.
- Red → 3f.

#### 3f. Merge-fixer subagent

Spawn `Agent(subagent_type: "general-purpose", model: "sonnet", description: "Fix integration failure for issue #<N>")` with:

- The failing command output (stderr/stdout).
- The issue body path.
- Hard rules: fix only the integration on the CURRENT feature branch. Do NOT revert the merge, do NOT broaden scope, do NOT touch unrelated code.
- Same output contract.

After the fixer returns:

- `SUCCESS` → re-run the same validation. Green → behave as 3e success. Still red → 3f-halt.
- `FAILED` → 3f-halt.

**3f-halt:** apply `needs-triage` to issue #N, comment with the failing output + fixer reason, halt the loop. Skip step 4's PR creation but still run step 5's cleanup.

#### 3g. Subagent failure

- Keep the sub-branch (do NOT delete).
- Remove `ready-for-agent`, apply `needs-triage`.
- Comment on the issue: "Failed during /orchestrate: <reason>" plus the subagent's last ~20 lines.
- `consecutive_failures += 1`. If `== 3`, halt the loop. Else next issue.

### 4. Finalize

If at least one issue succeeded AND the loop wasn't halted by 3f:

- `Skill('app-changelog')` to write the dated section.
- `Skill('git-commit')` to commit the changelog
- `git push origin feature/<name>`.
- `gh pr create --base master --head feature/<name> --title "<feature-branch-name>: N issues" --body <body>`.
  - Body bullets: every `ready-for-qa` issue with `#N`, title, URL, and its QA bullets.
  - Trailing section "Skipped/failed issues" listing any `needs-triage` ones from this run with their reason.
- Print the PR URL.

If no issues succeeded, do NOT open a PR. Report what failed.

### 5. Cleanup and report

- Remove `.tmp/orchestrate/`.
- Final report to user, with these sections:
  - **Succeeded** (`ready-for-qa`): list of `#N — title`.
  - **Failed** (`needs-triage`): `#N — reason`. Mention surviving sub-branches by name.
  - **Skipped** (cascade): `#N — blocked by #M`.
  - **Feature branch**: pushed or not; PR URL if opened.

## Halt conditions

- 3 consecutive subagent failures (3g).
- Merge-fixer cannot recover validation (3f-halt).
- Topological-sort cycle.
- Working tree dirty at start, or feature branch already exists.

## Resume

V1 does not support resuming a previous run. To re-attempt a `needs-triage` issue: fix the underlying issue, re-label it `ready-for-agent`, and run /orchestrate on a fresh feature branch name. (Future enhancement.)
