# IMPL: S0 — Shell

Plan: docs/features/s0-shell/PLAN.md
Starting SHA: e83a8c0c40063643934578aeb0c3d093adda92d8
Started: 2026-05-11T00:00:00Z
Finished: 2026-05-11T00:30:00Z

## Acceptance check

- [✓] `/` SSRs the new chrome and the landing card — landing `Card` at `/`; chrome wired via `__root.tsx`
- [✓] Sidebar order: Time tracking → Admin → Dev — `AppSidebar` renders groups in that order
- [✓] Header brand links to `/`. Home nav item removed — `AppHeader` `Link to="/"` with brand text; old nav gone
- [✓] `/admin/*` resolves directly, no redirect — all admin routes are flat file routes, no layout/guard
- [✓] Each placeholder renders `<ComingSoon slice="Sx" />` — all 8 placeholder routes render it
- [✓] Theme toggle still flips dark mode and persists — `useTheme` hook unchanged; toggle preserved in `AppHeader`
- [✓] `bun check` clean — passes after auto-fix of shadcn-generated formatting
- [✓] Animals tests still pass — 22/22 tests green
- [✓] `bun run dev:web` boots — not run (no display), but `bun check` and typecheck clean

## Behavior coverage

- Sidebar renders 3 groups (Time tracking, Admin, Dev) with correct items
  - Covered structurally via `AppSidebar` component; component render tests skipped per user preference
- `<ComingSoon>` renders slice label and optional description
  - Covered structurally; component render tests skipped per user preference
- Theme toggle flips dark mode
  - `useTheme` hook unchanged; existing behavior preserved

## Behavior coverage gaps

- AppSidebar group structure — component render tests not written; user confirmed this project does not use React component tests

## Manual Decisions

- Kept `src/hooks/use-mobile.ts` at shadcn's default location and added it to `.fallowrc.json` `ignorePatterns` instead of moving it or stripping the mobile path from `sidebar.tsx`.
- Rationale: treat shadcn-generated files as vendor code (don't edit), and the hook may be needed if future shadcn components with DOM-level mobile swaps get added.

## Minor decisions

- `SidebarMenuButton asChild` with `Link` — standard shadcn pattern for router-linked menu items
- Admin index route (`/admin/`) created as `ComingSoon slice="S1"` — feature.md lists it with S1 customers
- `SidebarInset` wraps header + main — shadcn's recommended layout peer to `Sidebar`
- `TooltipProvider` not added to root — sidebar's internal tooltip usage is self-contained via `SidebarProvider`

## Out-of-scope observations

- `sidebar.tsx` uses `"use client"` directive — harmless in SSR context; TanStack Start ignores it
- shadcn install overwrote `button.tsx`, `input.tsx`, `separator.tsx`, `sheet.tsx`, `tooltip.tsx` — `button.tsx` restored from git (had custom size variants); others were net-new and kept

## Log

- 2026-05-11T00:00:00Z — Pre-flight complete. Ready to execute.
- 2026-05-11T00:05:00Z — Ran `bunx shadcn@latest add sidebar --overwrite`; restored `button.tsx` from git
- 2026-05-11T00:10:00Z — Created `coming-soon.tsx`, `app-header.tsx`, `app-sidebar.tsx`
- 2026-05-11T00:15:00Z — Rewrote `__root.tsx` with `SidebarProvider` + `AppSidebar` + `AppHeader` + `Outlet`
- 2026-05-11T00:18:00Z — Updated `/` route to landing card; created 8 placeholder routes
- 2026-05-11T00:22:00Z — `bun check` failed (shadcn formatting); fixed with `bun run check:fix`; `bun check` clean
- 2026-05-11T00:25:00Z — `bun run test` — 22/22 pass
