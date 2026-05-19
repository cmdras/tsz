# S6 — OAuth (Entra ID)

## What

Wire Microsoft Entra ID (single-tenant, Euricom) authentication into the full stack:

- Frontend: Auth.js (`@auth/core`) with the `microsoft-entra-id` provider — login page, encrypted session cookie, route guard, token refresh.
- Backend: JWT Bearer middleware validating tokens against the Euricom tenant — all S1–S5 admin route groups require authorization.
- API client: Bearer token forwarded from the Auth.js session cookie to every outbound `.NET` API request.

## Reference implementation

`/home/chris/git/poc-tsz` contains a working proof-of-concept. Mirror it closely.

Key files to port:

- `packages/api/Common/Extensions/AuthExtensions.cs` — `AddEntraJwtAuth` / `UseEntraJwtAuth`
- `packages/web/src/lib/auth.server.ts` — Auth.js config, token storage, `getServerSession`, `getAccessToken`, inline refresh
- `packages/web/src/lib/auth-client.ts` — `signInWithMicrosoft`, `signOut`, `useSession`
- `packages/web/src/routes/api/auth/$.tsx` — Auth.js catch-all route
- `packages/web/src/routes/_authed.tsx` — route guard layout
- `packages/web/src/api/client.ts` — `authMiddleware` that attaches `Bearer` token

## Scope

### Backend

1. Add `AuthExtensions.cs` (copy from poc-tsz verbatim — `AddEntraJwtAuth` / `UseEntraJwtAuth`, `Auth:Disabled` escape hatch).
2. Wire into `Program.cs`: `builder.AddEntraJwtAuth()` + `app.UseEntraJwtAuth()`.
3. Add `RequireAuthorization()` (guarded by `Auth:Disabled`) to every existing endpoint group: Customers, Users, Contracts, LeaveTypes.
4. Add `Auth:TenantId`, `Auth:ClientId`, `Auth:Disabled` to `appsettings.json` (blank values) and `.env.example`.

### Frontend

5. Install `@auth/core`.
6. Add `auth.server.ts` — Auth.js config with `microsoft-entra-id` provider, `jwt` + `session` callbacks, `getServerSession`, `getAccessToken`, inline refresh, `profile()` override that drops the avatar.
7. Add `auth-client.ts` — `signInWithMicrosoft`, `signOut`, `useSession`.
8. Add `routes/api/auth/$.tsx` — catch-all GET/POST route delegating to `handleAuth`.
9. Add `routes/_authed.tsx` — `beforeLoad` guard; redirects to `/login` on missing/errored session; returns `{ session }` into context.
10. Add `routes/login.tsx` — login page with "Sign in with Microsoft" button; redirects to `/` if already authenticated.
11. Move existing admin routes under `routes/_authed/admin/` so the guard applies to all of them.
12. Update `api/client.ts` — add `authMiddleware` (`createIsomorphicFn` server path calls `getAccessToken`; client path throws).
13. Add `AUTH_CLIENT_ID`, `AUTH_CLIENT_SECRET`, `AUTH_TENANT_ID`, `AUTH_SECRET`, `APP_BASE_URL` to `.env.example`.

## Not in scope

- Role-based authorization (all authenticated Euricom users are equal for now).
- User profile page / avatar display.
- Sign-out button in the navbar (can be added later; the route guard handles expired sessions).
- The `Auth:Disabled` flag does not need a test environment — integration tests mock auth at the HTTP boundary.

## Azure app registration (already exists in Euricom tenant)

- Single tenant (`AUTH_TENANT_ID`)
- Redirect URI: `http://localhost:3000/api/auth/callback/microsoft-entra-id`
- Exposed API scope: `api://{CLIENT_ID}/access_as_user`
- `AUTH_SECRET`: 32-byte base64 string (`openssl rand -base64 32`)
