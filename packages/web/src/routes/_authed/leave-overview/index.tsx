import { createFileRoute } from '@tanstack/react-router';
import { fetchLeaveOverview } from '#/features/leave-overview/leave-overview.functions';
import { leaveOverviewSearchSchema } from '#/features/leave-overview/leave-overview.schemas';
import { BalanceSidebar } from './-components/balance-sidebar';
import { LegendChips } from './-components/legend-chips';
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
  const year = overview.year;
  const focus = search.focus;
  const focusedType = focus ? overview.types.find((leaveType) => leaveType.id === focus) : undefined;

  return (
    <div className="flex flex-col gap-6 p-6">
      <YearHeader year={year} types={overview.types} />
      <div className="flex flex-col gap-2">
        {focusedType && (
          <p className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">
            FOCUSED ON {focusedType.name.toUpperCase()}
          </p>
        )}
        <LegendChips types={overview.types} focus={focus} />
      </div>
      <div className="flex items-start gap-6">
        <YearCalendar year={year} types={overview.types} days={overview.days} focusedTypeName={focusedType?.name} />
        <BalanceSidebar year={year} types={overview.types} />
      </div>
    </div>
  );
}
