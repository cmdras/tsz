import { createFileRoute } from '@tanstack/react-router';
import { fetchUsers } from '#/features/users/users.functions';
import { searchSchema } from '#/features/users/users.schemas';
import { UserEmptyPanel } from './-components/user-empty-panel';
import { UsersPageLayout } from './-components/users-page-layout';

export const Route = createFileRoute('/_authed/admin/users/')({
  validateSearch: searchSchema,
  loaderDeps: ({ search }) => ({ search: search.search }),
  loader: ({ deps }) => fetchUsers({ data: { search: deps.search } }),
  staleTime: 30_000,
  component: UserList,
});

function UserList() {
  const { items } = Route.useLoaderData();
  const { search } = Route.useSearch();

  return (
    <UsersPageLayout users={items} search={search}>
      <UserEmptyPanel />
    </UsersPageLayout>
  );
}
