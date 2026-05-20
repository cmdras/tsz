import { createFileRoute, getRouteApi, useRouter } from '@tanstack/react-router';
import { UserDetailPanel } from '../-components/user-detail-panel';
import { UserNotFound } from '../-components/user-not-found';

const parentRoute = getRouteApi('/_authed/admin/users/$id');

export const Route = createFileRoute('/_authed/admin/users/$id/')({
  component: UserDetail,
});

function UserDetail() {
  const { id } = Route.useParams();
  const { items } = parentRoute.useLoaderData();
  const router = useRouter();

  const user = items.find((candidate) => candidate.id === id);

  if (!user) return <UserNotFound />;

  return <UserDetailPanel user={user} onArchiveSuccess={() => router.invalidate()} />;
}
