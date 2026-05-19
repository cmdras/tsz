import { useEffect, useState } from 'react';

const LOG_PREFIX = '[auth-client]';

type SessionUser = { id?: string; name?: string | null; email?: string | null };
type Session = { user?: SessionUser; expires?: string; error?: string };

async function fetchJson<T>(url: string): Promise<T> {
  console.log(`${LOG_PREFIX} → GET ${url}`);
  try {
    const res = await fetch(url, { credentials: 'same-origin' });
    console.log(`${LOG_PREFIX} ← ${res.status} ${url}`);
    if (!res.ok) throw new Error(`${url} failed: ${res.status}`);
    return (await res.json()) as T;
  } catch (err) {
    console.error(`${LOG_PREFIX} ✗ ${url}`, err);
    throw err;
  }
}

async function getCsrfToken(): Promise<string> {
  const { csrfToken } = await fetchJson<{ csrfToken: string }>('/api/auth/csrf');
  return csrfToken;
}

function submitForm(action: string, fields: Record<string, string>) {
  const form = document.createElement('form');
  form.method = 'POST';
  form.action = action;
  for (const [name, value] of Object.entries(fields)) {
    const input = document.createElement('input');
    input.type = 'hidden';
    input.name = name;
    input.value = value;
    form.appendChild(input);
  }
  document.body.appendChild(form);
  form.submit();
}

export async function signInWithMicrosoft(callbackUrl = '/') {
  console.log(`${LOG_PREFIX} → POST /api/auth/signin/microsoft-entra-id`);
  const csrfToken = await getCsrfToken();
  submitForm('/api/auth/signin/microsoft-entra-id', { csrfToken, callbackUrl });
}

export async function signOut(callbackUrl = '/login') {
  console.log(`${LOG_PREFIX} → POST /api/auth/signout`);
  const csrfToken = await getCsrfToken();
  submitForm('/api/auth/signout', { csrfToken, callbackUrl });
}

export function useSession(): { data: Session | null; isPending: boolean } {
  const [data, setData] = useState<Session | null>(null);
  const [isPending, setIsPending] = useState(true);
  useEffect(() => {
    let cancelled = false;
    fetchJson<Session>('/api/auth/session')
      .then((s) => {
        if (cancelled) return;
        // Auth.js returns `{}` for no session; normalize to null.
        setData(s && s.user ? s : null);
      })
      .catch(() => {
        if (!cancelled) setData(null);
      })
      .finally(() => {
        if (!cancelled) setIsPending(false);
      });
    return () => {
      cancelled = true;
    };
  }, []);
  return { data, isPending };
}
