---
name: worktree-merge
description: 'Merge a Claude Code worktree branch back into whatever branch the primary worktree currently has checked out (works for `main`/`master` or any feature branch), then remove the linked worktree via `ExitWorktree`.'
argument-hint: '[strategy] — one of: merge | rebase. Optional second arg: --keep-branch to skip deleting the branch after merge.'
disable-model-invocation: true
model: haiku
---

## Arguments

The skill accepts a strategy argument that selects which workflow to run:

| Argument           | Strategy         | When to use                                                       |
| ------------------ | ---------------- | ----------------------------------------------------------------- |
| _(none)_ / `merge` | `--no-ff` merge  | **Default.** One worktree = one task; preserves the branch point. |
| `rebase`           | Rebase + ff-only | Personal branch, linear history wanted.                           |

Optional flags (can follow the strategy):

- `--keep-branch` — do not delete the local/remote branch in Phase 4.
- `--no-push` — skip pushing the target branch to origin (e.g. for offline work).

If no argument is given, run the **default `--no-ff` merge** flow.

## Goal

Merge a linked worktree's branch into **whatever branch the primary worktree currently has checked out** (the "target branch") — could be `main`, `master`, or a feature branch the user is stacking work onto. Then remove the linked worktree via `ExitWorktree`. All target-branch ops use `git -C "$PRIMARY"` so the session's cwd can stay inside the linked worktree (Git refuses to check out the same branch in two places).

## Assumed starting state

- Claude Code session is running **inside a linked worktree** under `.claude/worktrees/<name>/` (created earlier via `EnterWorktree`).
- The worktree branch holds the finished work.
- The primary (root) worktree is on the **target branch** — whatever the user wants this work merged into.

## Phase 1 — Verify worktree is clean, read target branch from primary

**Do not stage or commit anything in this phase.** Committing is the user's responsibility — this skill only inspects.

The target branch is read directly from the primary worktree's `HEAD`. Whatever the user has checked out there is where this work goes.

```bash
BRANCH=$(git branch --show-current)
PRIMARY=$(git worktree list --porcelain | awk '/^worktree /{print $2; exit}')
TARGET=$(git -C "$PRIMARY" branch --show-current)

echo "branch=$BRANCH  target=$TARGET  primary=$PRIMARY"
git status --porcelain
```

Validate before continuing:

- **`$PRIMARY` equals current directory** → stop; the session is already in the primary worktree. Tell the user to run plain Git; this skill only applies inside a linked worktree.
- **Detached HEAD on linked worktree** (`$BRANCH` empty) → stop and ask the user to create a named branch (see `references/edge-cases.md`).
- **Detached HEAD on primary worktree** (`$TARGET` empty) → stop; the primary worktree isn't on a branch, so there's nothing to merge _into_. Ask the user to check out the target branch in the primary worktree first.
- **`$BRANCH` equals `$TARGET`** → stop; the worktree is on the same branch as primary — Git wouldn't have allowed this in the first place, but bail just in case.

Then branch on tree state:

- **Clean tree, branch ahead of `$TARGET`** → proceed to Phase 2.
- **Clean tree, no commits beyond `$TARGET`** → stop and tell the user there is nothing to merge.
- **Uncommitted changes** → **stop**. Do not stage, commit, or stash. Show a grouped summary from `git status --porcelain` + `git diff --stat HEAD` (added/modified/deleted/untracked, one bullet per file with a short hint inferred from path + diff). End with a single next step: commit, then re-run `/git-worktree-merge`. No raw porcelain dump.

Check "ahead of target" with:

```bash
git -C "$PRIMARY" rev-list --count "$TARGET..$BRANCH"
```

## Phase 2 — Merge into the target branch (operating on the primary worktree)

All commands here use `git -C "$PRIMARY"`. Primary is already on `$TARGET` — no `switch` needed. The linked worktree's `HEAD` stays put on `$BRANCH`; the branch must remain checked out somewhere for the merge to reference it.

```bash
git -C "$PRIMARY" fetch origin
# Only fast-forward if origin has the target branch (feature branches may be local-only)
if git -C "$PRIMARY" show-ref --verify --quiet "refs/remotes/origin/$TARGET"; then
  git -C "$PRIMARY" pull --ff-only origin "$TARGET"
fi
git -C "$PRIMARY" merge --no-ff "$BRANCH" -m "Merge branch '$BRANCH' into $TARGET"
```

`--no-ff` preserves the task branch as a visible integration point and fits "one task per worktree branch".

### If conflicts occur

Conflicts land in `$PRIMARY`, not cwd. Resolve there:

```bash
git -C "$PRIMARY" status
# edit files inside $PRIMARY
git -C "$PRIMARY" add <resolved-files>
git -C "$PRIMARY" commit
```

## Phase 3 — Push and verify

```bash
git -C "$PRIMARY" push origin "$TARGET"
git -C "$PRIMARY" log --oneline --decorate -n 15
```

`push` will create the remote branch if `$TARGET` is a new feature branch with no upstream yet — add `-u` once if you want it tracked.

If the worktree branch was previously pushed, delete the remote copy now (must happen **before** Phase 4 — `ExitWorktree(remove)` drops the local ref):

```bash
git -C "$PRIMARY" push origin --delete "$BRANCH" 2>/dev/null || true
```

## Phase 4 — Exit and remove the worktree

Call the `ExitWorktree` tool with:

```
action: "remove"
discard_changes: true
```

- `remove` deletes the linked worktree directory **and** the local `$BRANCH`. Safe now: commits are merged and pushed.
- `discard_changes: true` is needed because `ExitWorktree`'s safety check doesn't re-check against `$TARGET` after our external merge — but those commits are already on `$TARGET` (and on `origin/$TARGET` if pushed), so discarding the ref loses nothing.

## Rebase variant

If invoked with `rebase`, see [`references/rebase-variant.md`](references/rebase-variant.md).

## Decision guide

| Situation                                      | Best option                                         |
| ---------------------------------------------- | --------------------------------------------------- |
| Normal Claude Code worktree task               | `--no-ff` merge into primary's branch (default)     |
| Stacking work onto a feature branch in primary | Same default flow — `$TARGET` is the feature branch |
| Small personal branch, linear history          | Rebase variant                                      |
| Experimental detached worktree                 | Create a real branch first, then merge              |

## Universal rules

- Merge **branches**, not directories.
- Target branch = whatever the primary worktree has checked out. Never hardcode `main`/`master`.
- The linked worktree must be clean before Phase 2 — the skill never commits or stashes for the user.
- All target-branch operations use `git -C "$PRIMARY"`; do not `git switch "$TARGET"` inside the linked worktree (Git will refuse).
- Delete the remote feature branch (if any) **before** `ExitWorktree`, while the local ref still exists.
- Cleanup happens via one `ExitWorktree(action="remove", discard_changes=true)` call at the end — no separate `git worktree remove` step.

## Edge cases

Detached HEAD, stale metadata, moved worktree → see [`references/edge-cases.md`](references/edge-cases.md).

## Team policy (Claude Code)

- One task per worktree branch.
- No direct commits on the default branch from worktree sessions.
- Merge back only after local validation succeeds.
- Delete merged worktrees and branches promptly.
