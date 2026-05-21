#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT=$(git -C "$(dirname "${BASH_SOURCE[0]}")" rev-parse --show-toplevel)

name=$(jq -r '.name' <<< "$(cat)")
worktree="$REPO_ROOT/.claude/worktrees/$name"

git -C "$REPO_ROOT" worktree add "$worktree" -b "worktree-$name" HEAD >&2

copy_if_exists() {
  local src="$1" dst="$2"
  if [ -e "$src" ]; then
    mkdir -p "$(dirname "$dst")"
    cp "$src" "$dst"
  fi
}

copy_if_exists "$REPO_ROOT/packages/api/tsz.db"                       "$worktree/packages/api/tsz.db"
copy_if_exists "$REPO_ROOT/packages/api/appsettings.Development.json" "$worktree/packages/api/appsettings.Development.json"
copy_if_exists "$REPO_ROOT/packages/web/.env"                         "$worktree/packages/web/.env"
copy_if_exists "$REPO_ROOT/.claude/settings.local.json"               "$worktree/.claude/settings.local.json"

(cd "$worktree" && bun install) </dev/null >/dev/null 2>&1 &
disown 2>/dev/null || true

echo "cd $worktree" >&2
echo "$worktree"
