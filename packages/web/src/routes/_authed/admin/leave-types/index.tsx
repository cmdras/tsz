import { createFileRoute } from '@tanstack/react-router';
import { fetchLeaveTypes } from '#/features/leave-types/leave-types.functions';
import { leaveTypeSearchSchema } from '#/features/leave-types/leave-types.schemas';
import { LeaveTypeEmptyPanel } from './-components/leave-type-empty-panel';
import { LeaveTypesPageLayout } from './-components/leave-types-page-layout';

export const Route = createFileRoute('/_authed/admin/leave-types/')({
  validateSearch: leaveTypeSearchSchema,
  loaderDeps: ({ search }) => ({
    search: search.search,
    page: search.page,
    archived: search.archived,
  }),
  loader: ({ deps }) => fetchLeaveTypes({ data: deps }),
  staleTime: 30_000,
  component: LeaveTypeList,
});

function LeaveTypeList() {
  const { items, total } = Route.useLoaderData();
  const { search, page, archived } = Route.useSearch();

  return (
    <LeaveTypesPageLayout leaveTypes={items} total={total} search={search} page={page} archived={archived}>
      <LeaveTypeEmptyPanel />
    </LeaveTypesPageLayout>
  );
}
