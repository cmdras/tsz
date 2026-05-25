import { Outlet, createFileRoute, redirect } from '@tanstack/react-router';
import { createServerFn } from '@tanstack/react-start';
import { AppNavbar } from '#/components/app-navbar';
import { ApiRequestError, client } from '#/api/client';
import { getSessionServerFn } from '#/lib/session';

function returnNullIfUnauthorized(error: unknown): null {
  if (!(error instanceof ApiRequestError) || error.status !== 401) throw error;
  return null;
}

const getCurrentUser = createServerFn({ method: 'GET' }).handler(async () => {
  try {
    const { data } = await client.GET('/api/users/me');
    return data ?? null;
  } catch (error) {
    return returnNullIfUnauthorized(error);
  }
});

export const Route = createFileRoute('/_authed')({
  beforeLoad: async ({ location }) => {
    console.log(`[guard] beforeLoad → ${location.pathname}`);
    const session = await getSessionServerFn();
    console.log(
      `[guard] getSession → ${session?.user ? `user=${session.user.email ?? session.user.id}` : 'no session'}`,
    );
    if (!session || session.error === 'RefreshAccessTokenError') {
      console.log(`[guard] redirect → /login (from ${location.pathname})`);
      throw redirect({ to: '/login' });
    }
    const currentUser = await getCurrentUser();
    if (!currentUser) {
      throw redirect({ to: '/login', search: { error: 'api_unauthorized' } });
    }
    return { session, currentUser };
  },
  component: AuthedLayout,
});

function AuthedLayout() {
  return (
    <div className="flex min-h-screen flex-col">
      <AppNavbar />
      <main className="flex-1 p-6">
        <Outlet />
      </main>
    </div>
  );
}
