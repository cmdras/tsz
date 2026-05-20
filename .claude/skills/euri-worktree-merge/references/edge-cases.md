# Edge cases

## Detached HEAD

If the linked worktree was started with `--detach`, create a branch before Phase 2:

```bash
git switch -c feature/recover-work
```

## Recovery

- Stale worktree metadata after manual deletion: `git worktree prune`
- Moved worktree Git can't find: `git worktree repair`
