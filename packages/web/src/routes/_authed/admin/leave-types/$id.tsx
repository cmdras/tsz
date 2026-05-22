import { createFileRoute, Outlet } from '@tanstack/react-router';
import { fetchLeaveTypes } from '#/features/leave-types/leave-types.functions';
import { leaveTypeSearchSchema } from '#/features/leave-types/leave-types.schemas';
import { LeaveTypesPageLayout } from './-components/leave-types-page-layout';

export const Route = createFileRoute('/_authed/admin/leave-types/$id')({
  validateSearch: leaveTypeSearchSchema,
  loaderDeps: ({ search }) => ({
    search: search.search,
    page: search.page,
    archived: search.archived,
  }),
  loader: ({ deps }) => fetchLeaveTypes({ data: deps }),
  staleTime: 30_000,
  component: LeaveTypeDetailLayout,
});

function LeaveTypeDetailLayout() {
  const { items, total } = Route.useLoaderData();
  const { id } = Route.useParams();
  const { search, page, archived } = Route.useSearch();

  return (
    <LeaveTypesPageLayout
      leaveTypes={items}
      total={total}
      selectedId={id}
      search={search}
      page={page}
      archived={archived}
    >
      <Outlet />
    </LeaveTypesPageLayout>
  );
}
