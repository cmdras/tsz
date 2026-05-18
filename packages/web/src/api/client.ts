import createClient, { type Middleware } from 'openapi-fetch';
import { type paths } from './schema';

export class ApiRequestError extends Error {
  constructor(
    public status: number,
    public body?: string,
  ) {
    super(`HTTP ${status}`);
  }
}

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

export const client = createClient<paths>({ baseUrl: process.env.SERVER_URL ?? 'http://localhost:5204' });
client.use(errorMiddleware);
