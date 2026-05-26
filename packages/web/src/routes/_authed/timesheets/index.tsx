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

  const [year, month] = yearMonth.split('-').map(Number);
  const monthLabel = new Date(year, month - 1, 1).toLocaleDateString('en-GB', { month: 'long', year: 'numeric' });

  return (
    <div className="flex h-[calc(100vh-7rem)] flex-col gap-6 p-6">
      <div className="flex items-start justify-between">
        <div>
          <h1 className="text-2xl font-semibold">
            Timesheets <em className="font-normal text-primary">this month.</em>
          </h1>
          <p className="mt-0.5 text-sm text-muted-foreground">{monthLabel}</p>
        </div>
        <MonthNav yearMonth={yearMonth} />
      </div>

      <div className="flex min-h-0 flex-1 gap-4">
        <div className="flex h-full min-w-0 flex-1 flex-col">
          <MonthGrid days={monthData.days} today={today} weekSubmissions={monthData.weekSubmissions} />
        </div>
        <MonthSidebar days={monthData.days} weekSubmissions={monthData.weekSubmissions} today={today} />
      </div>
    </div>
  );
}
