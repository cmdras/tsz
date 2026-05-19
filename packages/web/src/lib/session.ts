import { createServerFn } from '@tanstack/react-start';
import { getRequest } from '@tanstack/react-start/server';
import { getServerSession } from '#/lib/auth.server';

export const getSessionServerFn = createServerFn({ method: 'GET' }).handler(async () => {
  const request = getRequest();
  if (!request) return null;
  return getServerSession(request.headers);
});
