import { createFileRoute } from '@tanstack/react-router';
import { handleAuth } from '#/lib/auth.server';

export const Route = createFileRoute('/api/auth/$')({
  server: {
    handlers: {
      GET: ({ request }: { request: Request }) => handleAuth(request),
      POST: ({ request }: { request: Request }) => handleAuth(request),
    },
  },
});
