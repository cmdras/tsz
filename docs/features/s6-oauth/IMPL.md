# S6 — Entra ID OAuth across API and web

[PLAN.md](PLAN.md) | Started: 2026-05-18 | Chapter: 6/6

## Chapter 1: Tests — done

- **Tests:** 4 new integration tests (all fail: `Expected: Unauthorized / Actual: OK`). 96 unit tests green, 56–57 existing integration tests green (5–6 pre-existing LeaveType/isolation failures are non-deterministic and pre-date this slice).
- **Deviation:** Created `packages/api.tests.integration/` project as a new `IClassFixture<AuthEnforcedApiFactory>` (dedicated subclass of `WebApplicationFactory<Program>`) rather than delegating from the shared `WebApplicationFactory<Program>` fixture, to avoid cross-class SQLite startup conflicts that were already present in the pre-existing test suite.

## Chapter 2: Schema + EF — skipped

- No new entity or migration. OAuth is pure middleware configuration.

## Chapter 3: Service — done

- **Tests:** 96 unit tests passing, 0 regressions.
- **Manual:** Fill `Auth:TenantId` and `Auth:ClientId` in `appsettings.Development.json` (or environment variables) before running the API locally with auth enabled.

## Chapter 4: Endpoints — done

- **Tests:** 4 auth enforcement failing → 0 failing, 0 regressions (62 existing + 4 new = 66 total passing).
- **Deviation:** PLAN's `appsettings.Testing.json` approach for disabling auth in tests doesn't work — `WebApplicationFactory.ConfigureAppConfiguration` runs at `Build()` time, but `AddEntraJwtAuth` reads config before that (at bootstrap), so both factories saw `Auth:Disabled=true` from the appsettings file. Fixed by emptying `appsettings.Testing.json` and moving `Auth:Disabled=true` into `TestApiFactory.ConfigureAppConfiguration` (same timing as the factory override), making builder and middleware reads consistent.

## Chapter 5: FE data access — done

- **Deviation:** Added `src/env.server.ts` (Zod-validated env schema) not in the original PLAN; ported from poc-tsz at user's direction. `client.ts` uses `env.SERVER_URL` from this file rather than `process.env.SERVER_URL`.
- **Manual:** Copy `packages/web/.env.example` to `packages/web/.env` and fill `AUTH_TENANT_ID`, `AUTH_CLIENT_ID`, `AUTH_CLIENT_SECRET`, `AUTH_SECRET` before running the web app.

## Chapter 6: FE UI — done

- **Manual:** `SERVER_URL` in `packages/web/.env` must also be set (e.g. `http://localhost:5204`) before running the web app.

## Acceptance check

- ✓ Unauthenticated visit to any URL except `/login` and `/api/auth/*` redirects to `/login` — `_authed.tsx` `beforeLoad` throws `redirect({ to: '/login' })` on null session or `RefreshAccessTokenError`.
- ✗ "Sign in with Microsoft" completes the Entra flow and returns the user to `/` — requires real Entra credentials in `.env`; not verifiable without them.
- ✗ Admin pages load `/api/*` data successfully when authenticated — requires live API + auth; manually testable once `.env` is filled.
- ✓ Token expiring within 60s triggers inline refresh — implemented in `getAccessToken` and the `jwt` callback in `auth.server.ts`.
- ✓ Refresh failure stamps `RefreshAccessTokenError` and bounces user to `/login` — `_authed.tsx` `beforeLoad` checks `session.error === 'RefreshAccessTokenError'`.
- ✓ Sign-out item in navbar dropdown clears session and redirects to `/login` — `AppNavbar` calls `signOut('/login')` via `auth-client.ts`.
- ✓ All existing `api.tests.integration` tests pass — 66/66 passing, 0 regressions.
- ✓ Booting API without `Auth:TenantId`/`Auth:ClientId` outside Testing env throws `InvalidOperationException` — `AddEntraJwtAuth` throws on missing config.
- ✓ `AppNavbar` does not render on `/login` — navbar lives in `_authed.tsx` layout; `/login` is a sibling route under root.
