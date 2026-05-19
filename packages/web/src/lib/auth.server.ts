import { Auth, type AuthConfig } from '@auth/core';
import { getToken } from '@auth/core/jwt';
import MicrosoftEntraID from '@auth/core/providers/microsoft-entra-id';
import { env } from '#/env.server';

const LOG_PREFIX = '[auth]';

const useSecureCookies = env.APP_BASE_URL.startsWith('https://');
const COOKIE_NAME = useSecureCookies ? '__Secure-authjs.session-token' : 'authjs.session-token';
const SCOPE = `openid profile email offline_access api://${env.AUTH_CLIENT_ID}/access_as_user`;

function ts() {
  return new Date().toISOString();
}

type AuthToken = {
  sub?: string;
  name?: string | null;
  email?: string | null;
  access_token?: string;
  refresh_token?: string;
  expires_at?: number;
  error?: 'RefreshAccessTokenError';
};

async function refreshAccessToken(refreshToken: string): Promise<{
  access_token: string;
  refresh_token: string;
  expires_at: number;
}> {
  const res = await fetch(`https://login.microsoftonline.com/${env.AUTH_TENANT_ID}/oauth2/v2.0/token`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: new URLSearchParams({
      client_id: env.AUTH_CLIENT_ID,
      client_secret: env.AUTH_CLIENT_SECRET,
      grant_type: 'refresh_token',
      refresh_token: refreshToken,
      scope: SCOPE,
    }),
  });
  if (!res.ok) throw new Error(`refresh failed: ${res.status}`);
  const data = (await res.json()) as { access_token: string; refresh_token?: string; expires_in: number };
  return {
    access_token: data.access_token,
    refresh_token: data.refresh_token ?? refreshToken,
    expires_at: Math.floor(Date.now() / 1000) + data.expires_in,
  };
}

export const authConfig: AuthConfig = {
  secret: env.AUTH_SECRET,
  trustHost: true,
  basePath: '/api/auth',
  useSecureCookies,
  session: { strategy: 'jwt' },
  providers: [
    MicrosoftEntraID({
      clientId: env.AUTH_CLIENT_ID,
      clientSecret: env.AUTH_CLIENT_SECRET,
      issuer: `https://login.microsoftonline.com/${env.AUTH_TENANT_ID}/v2.0`,
      authorization: { params: { scope: SCOPE, prompt: 'select_account' } },
      // Override default profile() to skip the Graph /me/photos call.
      // Default fetches a 48×48 photo and base64-embeds it (~10KB) into
      // user.image → token.picture → JWE cookie → cookie chunking.
      profile(profile) {
        return {
          id: (profile.oid as string | undefined) ?? (profile.sub as string),
          name: (profile.name as string | undefined) ?? null,
          email: (profile.email as string | undefined) ?? (profile.preferred_username as string | undefined) ?? null,
          image: null,
        };
      },
    }),
  ],
  callbacks: {
    async jwt({ token, account, profile }) {
      // Always strip picture/image — Auth.js copies user.image → token.picture
      // by default. Belt-and-braces alongside the provider's profile() override:
      // keeps the JWE cookie well under 4KB so it isn't chunked.
      const raw = token as Record<string, unknown>;
      delete raw.picture;
      delete raw.image;

      if (account) {
        const t = token as AuthToken;
        t.access_token = account.access_token as string | undefined;
        t.refresh_token = account.refresh_token as string | undefined;
        t.expires_at = account.expires_at as number | undefined;
        if (profile) {
          t.name = (profile.name as string | undefined) ?? t.name ?? null;
          t.email = (profile.email as string | undefined) ?? t.email ?? null;
          t.sub = (profile.oid as string | undefined) ?? (profile.sub as string | undefined) ?? t.sub;
        }
        return t;
      }

      const t = token as AuthToken;
      if (!t.expires_at || !t.refresh_token) return t;
      if (Math.floor(Date.now() / 1000) < t.expires_at - 60) return t;

      try {
        const refreshed = await refreshAccessToken(t.refresh_token);
        t.access_token = refreshed.access_token;
        t.refresh_token = refreshed.refresh_token;
        t.expires_at = refreshed.expires_at;
        delete t.error;
      } catch (err) {
        console.error(`${ts()} ${LOG_PREFIX} refresh failed`, err);
        t.error = 'RefreshAccessTokenError';
      }
      return t;
    },
    async session({ session, token }) {
      const t = token as AuthToken;
      if (session.user) {
        const u = session.user as { name: string; email: string; id?: string };
        u.name = t.name ?? '';
        u.email = t.email ?? '';
        u.id = t.sub;
      }
      (session as { error?: string }).error = t.error;
      return session;
    },
  },
};

export async function handleAuth(request: Request): Promise<Response> {
  const url = new URL(request.url);
  console.log(`${ts()} ${LOG_PREFIX} → ${request.method} ${url.pathname}${url.search}`);
  const response = await Auth(request, authConfig);
  console.log(`${ts()} ${LOG_PREFIX} ← ${request.method} ${url.pathname} status=${response.status}`);
  return response;
}

export type ServerSession = {
  user: { id?: string; name?: string | null; email?: string | null };
  accessToken?: string;
  error?: 'RefreshAccessTokenError';
};

async function readToken(headers: Headers): Promise<AuthToken | null> {
  const req = new Request('http://x', { headers });
  const token = (await getToken({
    req,
    secret: env.AUTH_SECRET,
    salt: COOKIE_NAME,
  })) as AuthToken | null;
  return token;
}

export async function getServerSession(headers: Headers): Promise<ServerSession | null> {
  const token = await readToken(headers);
  if (!token || !token.sub) return null;
  return {
    user: { id: token.sub, name: token.name ?? null, email: token.email ?? null },
    accessToken: token.access_token,
    error: token.error,
  };
}

export async function getAccessToken(headers: Headers): Promise<string | null> {
  const token = await readToken(headers);
  if (!token || token.error) return null;
  if (!token.access_token) return null;
  // getToken() only decodes the cookie; the jwt callback that refreshes
  // expired Microsoft tokens runs in Auth() handlers, not here. So if the
  // access_token in the cookie has expired, refresh inline. The refreshed
  // value isn't persisted back to the cookie — the next /api/auth/session
  // call will do that — so consecutive expired-cookie requests may each
  // refresh, which is acceptable for this app's traffic shape.
  if (token.expires_at && Math.floor(Date.now() / 1000) >= token.expires_at - 60 && token.refresh_token) {
    try {
      const refreshed = await refreshAccessToken(token.refresh_token);
      return refreshed.access_token;
    } catch (err) {
      console.error(`${ts()} ${LOG_PREFIX} inline refresh failed`, err);
      return null;
    }
  }
  return token.access_token;
}
