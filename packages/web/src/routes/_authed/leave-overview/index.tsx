import { createFileRoute } from '@tanstack/react-router';
import { fetchLeaveOverview } from '#/features/leave-overview/leave-overview.functions';
import { leaveOverviewSearchSchema } from '#/features/leave-overview/leave-overview.schemas';
import { YearHeader } from './-components/year-header';

function currentYear(): number {
  return new Date().getFullYear();
}

export const Route = createFileRoute('/_authed/leave-overview/')({
  validateSearch: leaveOverviewSearchSchema,
  loaderDeps: ({ search }) => ({
    year: search.year ?? currentYear(),
  }),
  loader: async ({ deps }) => {
    const overview = await fetchLeaveOverview({ data: { year: deps.year } });
    return { overview };
  },
  component: LeaveOverviewPage,
});

function LeaveOverviewPage() {
  const { overview } = Route.useLoaderData();
  const search = Route.useSearch();
  const year = search.year ?? currentYear();

  return (
    <div className="flex flex-col gap-6 p-6">
      <YearHeader year={year} types={overview.types} />
    </div>
  );
}
