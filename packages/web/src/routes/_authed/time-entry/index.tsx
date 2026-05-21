import { createFileRoute } from '@tanstack/react-router';
import { fetchPickerOptions, fetchWeek } from '#/features/time-entries/time-entries.functions';
import { weekSearchSchema } from '#/features/time-entries/time-entries.schemas';
import { getIsoMonday, toIsoDateString } from '#/lib/date-utils';
import { WeekGrid } from './-components/week-grid';
import { WeekNav } from './-components/week-nav';

export const Route = createFileRoute('/_authed/time-entry/')({
  validateSearch: weekSearchSchema,
  loaderDeps: ({ search }) => ({
    week: search.week ?? toIsoDateString(getIsoMonday(new Date())),
  }),
  loader: async ({ deps }) => {
    const [weekData, pickerOptions] = await Promise.all([
      fetchWeek({ data: { week: deps.week } }),
      fetchPickerOptions({ data: { week: deps.week } }),
    ]);
    return { weekData, pickerOptions };
  },
  component: TimeEntryPage,
});

function TimeEntryPage() {
  const { weekData, pickerOptions } = Route.useLoaderData();

  return (
    <div className="flex flex-col gap-6 p-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">
          Time entry <em className="font-normal text-muted-foreground">empty.</em>
        </h1>
        <WeekNav weekStart={weekData.weekStart} />
      </div>

      <div key={weekData.weekStart} className="animate-fade-in">
        <WeekGrid weekStart={weekData.weekStart} pickerOptions={pickerOptions} />
      </div>
    </div>
  );
}
