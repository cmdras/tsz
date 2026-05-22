const LOG_PREFIX = '[auth-client]';

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
