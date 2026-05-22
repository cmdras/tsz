import { createFileRoute, Outlet } from '@tanstack/react-router';
import { fetchUsers } from '#/features/users/users.functions';
import { userSearchSchema } from '#/features/users/users.schemas';
import { UsersPageLayout } from './-components/users-page-layout';

export const Route = createFileRoute('/_authed/admin/users/$id')({
  validateSearch: userSearchSchema,
  loaderDeps: ({ search }) => ({ search: search.search }),
  loader: ({ deps }) => fetchUsers({ data: { search: deps.search } }),
  staleTime: 30_000,
  component: UserDetailLayout,
});

function UserDetailLayout() {
  const { items } = Route.useLoaderData();
  const { id } = Route.useParams();
  const { search } = Route.useSearch();

  return (
    <UsersPageLayout users={items} selectedId={id} search={search}>
      <Outlet />
    </UsersPageLayout>
  );
}
