import { createFileRoute } from '@tanstack/react-router';
import { fetchMonth } from '#/features/timesheets/timesheets.functions';
import { monthSearchSchema } from '#/features/timesheets/timesheets.schemas';
import { toIsoDateString } from '#/lib/date-utils';
import { MonthGrid } from './-components/month-grid';
import { MonthNav } from './-components/month-nav';
import { MonthSidebar } from './-components/month-sidebar';

function currentYearMonth(): string {
  const now = new Date();
  const year = now.getFullYear();
  const month = String(now.getMonth() + 1).padStart(2, '0');
  return `${year}-${month}`;
}

function todayIsoString(): string {
  return toIsoDateString(new Date());
}

export const Route = createFileRoute('/_authed/timesheets/')({
  validateSearch: monthSearchSchema,
  loaderDeps: ({ search }) => ({
    month: search.month ?? currentYearMonth(),
  }),
  loader: async ({ deps }) => {
    const monthData = await fetchMonth({ data: { month: deps.month } });
    return { monthData, today: todayIsoString() };
  },
  component: TimesheetsPage,
});

function TimesheetsPage() {
  const { monthData, today } = Route.useLoaderData();
  const search = Route.useSearch();
  const yearMonth = search.month ?? currentYearMonth();

  return (
    <div className="flex flex-col gap-6 p-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">Timesheets</h1>
        <MonthNav yearMonth={yearMonth} />
      </div>

      <div className="flex gap-4">
        <div className="min-w-0 flex-1">
          <MonthGrid days={monthData.days} today={today} />
        </div>
        <MonthSidebar days={monthData.days} />
      </div>
    </div>
  );
}
