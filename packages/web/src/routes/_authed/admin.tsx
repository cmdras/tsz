import { Outlet, createFileRoute, redirect } from '@tanstack/react-router';

export const Route = createFileRoute('/_authed/admin')({
  beforeLoad: ({ context }) => {
    if (context.currentUser?.role !== 'Admin') {
      throw redirect({ to: '/' });
    }
  },
  component: () => <Outlet />,
});
