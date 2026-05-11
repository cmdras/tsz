# PLAN: S0 — Shell

Source: `feature.md`

## Approach

Rewrite `__root.tsx` from the toy nav into the real shell: `<SidebarProvider>` wrapping `<AppHeader>` + `<AppSidebar>` + `<Outlet>`. No loader, no route context — no identity to fetch. The `.NET` project is untouched in S0.

`<AppHeader>` — brand left, theme toggle right (preserve existing dark-mode behavior).

`<AppSidebar>` — three `<SidebarGroup>` blocks wired with TanStack Router `<Link>`. Admin section unconditional. `Home` link dropped; Animals is the sole Dev entry.

`<ComingSoon slice="Sx" description?="..." />` — shadcn `Card` with slice label + one-line note. Each placeholder route is a 3-line file rendering it.

`/admin/*` has no layout route, no `beforeLoad`, no guard. S6 adds them when auth exists.

## shadcn installs

`bunx shadcn@latest add sidebar card`.

## Tests

- `app-sidebar.test.tsx` — renders all 3 sections with expected items.

## Acceptance criteria

- `/` SSRs the new chrome and the landing card.
- Sidebar order: Time tracking → Admin → Dev.
- Header brand links to `/`. `Home` nav item removed.
- `/admin/*` resolves directly, no redirect.
- Each placeholder renders `<ComingSoon slice="Sx" />`.
- Theme toggle still flips dark mode and persists.
- `bun check` clean; animals tests still pass; `bun run dev:web` boots.

## Risks

- shadcn sidebar needs `<SidebarProvider>` + CSS variables — verify the install wires them.
