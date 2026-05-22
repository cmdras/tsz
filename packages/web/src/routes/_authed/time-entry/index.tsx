import { createFileRoute, useBlocker } from '@tanstack/react-router';
import { useEffect, useRef, useState } from 'react';
import { toast } from 'sonner';
import { Button } from '#/components/ui/button';
import { fetchPickerOptions, fetchWeek, saveDraft } from '#/features/time-entries/time-entries.functions';
import { weekSearchSchema } from '#/features/time-entries/time-entries.schemas';
import { getIsoMonday, toIsoDateString } from '#/lib/date-utils';
import { WeekGrid, type WeekGridHandle } from './-components/week-grid';
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
  const navigate = Route.useNavigate();
  const [isDirty, setIsDirty] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const gridRef = useRef<WeekGridHandle>(null);

  useBlocker({
    blockerFn: () => !window.confirm('You have unsaved changes. Leave anyway?'),
    condition: isDirty,
  });

  useEffect(() => {
    setIsDirty(false);
  }, [weekData.weekStart]);

  useEffect(() => {
    const handler = (event: BeforeUnloadEvent) => {
      if (isDirty) event.preventDefault();
    };
    window.addEventListener('beforeunload', handler);
    return () => window.removeEventListener('beforeunload', handler);
  }, [isDirty]);

  const hasRows = weekData.rows.length > 0;

  async function handleSaveDraft() {
    if (!gridRef.current) return;
    const cells = gridRef.current.getCells();
    setIsSaving(true);
    try {
      await saveDraft({ data: { week: weekData.weekStart, cells } });
      gridRef.current?.resetDirty();
      setIsDirty(false);
      await navigate({ search: (previous) => previous });
      toast.success('Draft saved.');
    } catch {
      toast.error('Failed to save draft.');
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <div className="flex flex-col gap-6 p-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">
          Time entry <em className="font-normal text-primary">{hasRows ? 'logged.' : 'empty.'}</em>
        </h1>
        <div className="flex items-center gap-3">
          {isDirty && (
            <Button onClick={handleSaveDraft} disabled={isSaving} size="sm">
              {isSaving ? 'Saving…' : 'Save draft'}
            </Button>
          )}
          <WeekNav weekStart={weekData.weekStart} />
        </div>
      </div>

      <div key={weekData.weekStart} className="animate-fade-in">
        <WeekGrid
          ref={gridRef}
          weekStart={weekData.weekStart}
          savedRows={weekData.rows}
          pickerOptions={pickerOptions}
          onDirtyChange={setIsDirty}
        />
      </div>
    </div>
  );
}
