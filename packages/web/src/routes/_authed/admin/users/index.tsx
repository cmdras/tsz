import { createFileRoute } from '@tanstack/react-router';
import { fetchUsers } from '#/features/users/users.functions';
import { userSearchSchema } from '#/features/users/users.schemas';
import { UserEmptyPanel } from './-components/user-empty-panel';
import { UsersPageLayout } from './-components/users-page-layout';

export const Route = createFileRoute('/_authed/admin/users/')({
  validateSearch: userSearchSchema,
  loaderDeps: ({ search }) => ({ search: search.search, filter: search.filter }),
  loader: ({ deps }) => fetchUsers({ data: { search: deps.search, filter: deps.filter } }),
  staleTime: 30_000,
  component: UserList,
});

function UserList() {
  const { items } = Route.useLoaderData();
  const { search, filter } = Route.useSearch();

  return (
    <UsersPageLayout users={items} search={search} filter={filter}>
      <UserEmptyPanel />
    </UsersPageLayout>
  );
}
