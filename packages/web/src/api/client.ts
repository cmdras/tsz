import createClient, { type Middleware } from 'openapi-fetch';
import { createIsomorphicFn } from '@tanstack/react-start';
import { getRequest } from '@tanstack/react-start/server';
import { type paths } from './schema';
import { getAccessToken } from '#/lib/auth.server';
import { env } from '#/env.server';

export class ApiRequestError extends Error {
  constructor(
    public status: number,
    public body?: string,
  ) {
    super(`HTTP ${status}`);
  }
}

const fetchAccessToken = createIsomorphicFn()
  .client((): Promise<string | null> => {
    throw new Error('API client called outside a server context — wrap the call in createServerFn().handler()');
  })
  .server(async (): Promise<string | null> => {
    const req = getRequest()!;
    return getAccessToken(req.headers);
  });

const authMiddleware: Middleware = {
  async onRequest({ request }) {
    const token = await fetchAccessToken();
    console.log('&&&&', token);
    if (token) request.headers.set('Authorization', `Bearer ${token}`);
    return request;
  },
};

export const errorMiddleware: Middleware = {
  async onResponse({ response }) {
    if (!response.ok) {
      const body = await response
        .clone()
        .text()
        .catch(() => undefined);
      throw new ApiRequestError(response.status, body);
    }
  },
};

export const client = createClient<paths>({ baseUrl: env.SERVER_URL });
client.use(authMiddleware);
client.use(errorMiddleware);
