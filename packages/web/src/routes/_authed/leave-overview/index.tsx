import { createFileRoute } from '@tanstack/react-router';
import { fetchLeaveOverview } from '#/features/leave-overview/leave-overview.functions';
import { leaveOverviewSearchSchema } from '#/features/leave-overview/leave-overview.schemas';
import { BalanceSidebar } from './-components/balance-sidebar';
import { YearCalendar } from './-components/year-calendar';
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
  const focus = search.focus;

  return (
    <div className="flex flex-col gap-6 p-6">
      <YearHeader year={year} types={overview.types} />
      <div className="flex items-start gap-6">
        <BalanceSidebar year={year} types={overview.types} />
        <YearCalendar year={year} types={overview.types} days={overview.days} focus={focus} />
      </div>
    </div>
  );
}
