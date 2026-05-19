# PLAN: S6 — Entra ID OAuth across API and web

Source: feature.md

## Goal
Wire Microsoft Entra ID (single-tenant, Euricom) authentication into both halves of the stack. The .NET API enforces JWT Bearer on every route group; the web app signs users in via Auth.js, holds tokens in an encrypted server-side cookie, and forwards them as `Bearer` on outbound API calls. Mirrors the working poc-tsz implementation.

## Approach

**Backend.** Port `Common/Extensions/AuthExtensions.cs` from poc-tsz (`AddEntraJwtAuth` / `UseEntraJwtAuth`, `Auth:Disabled` flag). Wire into `Program.cs`: `builder.AddEntraJwtAuth()` after `WebApplication.CreateBuilder`, `app.UseEntraJwtAuth()` after `Build()`. In each of the four endpoint files (Customers, Users, Contracts, LeaveTypes), add `if (!app.Configuration.GetValue<bool>(AuthExtensions.DisabledKey)) group.RequireAuthorization();` directly after the existing `MapApiGroup(...)` call. Add empty `Auth:TenantId` / `Auth:ClientId` keys to `appsettings.json` (no `Auth:Disabled` there — missing config in non-Testing envs throws at startup). Add `appsettings.Testing.json` with `Auth:Disabled=true` so the existing `WebApplicationFactory<Program>` fixture (which calls `UseEnvironment("Testing")`) keeps working unchanged. No per-test changes.

**Frontend — Auth.js plumbing.** Install `@auth/core`. Port three files from poc-tsz verbatim apart from imports: `src/lib/auth.server.ts` (provider config, `jwt`/`session` callbacks, inline `refreshAccessToken`, `getServerSession`, `getAccessToken`, `profile()` override that drops the avatar, belt-and-braces `delete raw.picture`/`raw.image` in `jwt`), `src/lib/auth-client.ts` (`signInWithMicrosoft`, `signOut`, `useSession`), and `src/routes/api/auth/$.tsx` (catch-all GET/POST → `handleAuth`).

**Frontend — route restructure.** Move every current top-level route (`index.tsx`, `admin/`, `time-entry/`, `timesheets/`, `leave-overview/`) under a new `routes/_authed/` directory. Add `routes/_authed.tsx` with a `beforeLoad` that calls a `createServerFn`-wrapped `getServerSession`; redirect to `/login` on null session or `RefreshAccessTokenError`. Relocate `AppNavbar` from `__root.tsx` into `_authed.tsx` so it only renders for authed users; `__root.tsx` keeps only the HTML shell, `Toaster`, theme script, and `<Outlet />`. Add `routes/login.tsx` — standalone shadcn `Card` centered on a blank page, "Sign in with Microsoft" button, mirror-guard that redirects to `/` if already authenticated. Extend `AppNavbar` with a small shadcn `DropdownMenu` user-menu (avatar/initials + name + email + "Sign out" item) sourced from `useSession()`; Sign out calls `signOut()` from `auth-client.ts`.

**Frontend — API client.** Update `src/api/client.ts`: add an `authMiddleware` whose `onRequest` calls a `createIsomorphicFn`-wrapped `fetchAccessToken` (server impl calls `getAccessToken(getRequest()!.headers)`; client impl throws with a clear "call from a server function" message). Attach `Authorization: Bearer <token>` when present. Keep the existing `errorMiddleware`. The throwing client path is a guardrail: any accidental browser-side API call surfaces immediately rather than silently dropping auth.

**Config.** Add `packages/web/.env.example` with `SERVER_URL`, `APP_BASE_URL`, `AUTH_CLIENT_ID`, `AUTH_CLIENT_SECRET`, `AUTH_TENANT_ID`, `AUTH_SECRET`. `useSecureCookies` derived from `APP_BASE_URL.startsWith('https://')` (matches poc-tsz). `SCOPE = 'openid profile email offline_access api://${CLIENT_ID}/access_as_user'`.

**Out of scope.** Role-based authorization (all authenticated Euricom users equal). User profile page. Avatar display beyond initials. Auth in unit-test project (`packages/api.tests`) since those test services directly, not HTTP.

## Acceptance criteria
- Unauthenticated visit to any URL except `/login` and `/api/auth/*` redirects to `/login`.
- "Sign in with Microsoft" completes the Entra flow and returns the user to `/`.
- Admin pages load `/api/*` data successfully when authenticated; the .NET API returns `401` for any request without a valid Bearer token (verified by hitting it directly with `curl`).
- Token expiring within 60s triggers inline refresh on the next API call; refresh failure stamps `RefreshAccessTokenError` and bounces the user to `/login` on the next navigation.
- Sign-out item in the navbar dropdown clears the session cookie and redirects to `/login`.
- All existing `api.tests.integration` tests pass with no per-test modifications.
- Booting the API without `Auth:TenantId` or `Auth:ClientId` outside the `Testing` environment throws `InvalidOperationException` at startup.
- `AppNavbar` does not render on `/login`.
