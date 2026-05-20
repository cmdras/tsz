# Rebase variant

Use only when rewriting branch history is acceptable (small personal branch, linear history).

`$TARGET` is the branch checked out in the primary worktree (see Phase 1 of `SKILL.md`).

```bash
# Phase 2 — still inside the linked worktree
git fetch origin
# Rebase onto origin's $TARGET if it exists, else onto local $TARGET
if git show-ref --verify --quiet "refs/remotes/origin/$TARGET"; then
  git rebase "origin/$TARGET"
else
  git rebase "$TARGET"
fi
git push --force-with-lease -u origin "$BRANCH"        # only if previously pushed

# Primary is already on $TARGET — no switch needed
if git -C "$PRIMARY" show-ref --verify --quiet "refs/remotes/origin/$TARGET"; then
  git -C "$PRIMARY" pull --ff-only origin "$TARGET"
fi
git -C "$PRIMARY" merge --ff-only "$BRANCH"
git -C "$PRIMARY" push origin "$TARGET"
git -C "$PRIMARY" push origin --delete "$BRANCH" 2>/dev/null || true

# Phase 4: ExitWorktree(action="remove", discard_changes=true)
```
