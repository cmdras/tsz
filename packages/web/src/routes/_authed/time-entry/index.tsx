import { createFileRoute, useBlocker } from '@tanstack/react-router';
import { useEffect, useRef, useState } from 'react';
import { toast } from 'sonner';
import { fetchPickerOptions, fetchWeek, saveDraft, submitWeekFn } from '#/features/time-entries/time-entries.functions';
import { weekSearchSchema } from '#/features/time-entries/time-entries.schemas';
import { getIsoMonday, toIsoDateString } from '#/lib/date-utils';
import { CopyConfirmDialog } from './-components/copy-confirm-dialog';
import { LastWeekCard } from './-components/last-week-card';
import { LoggedCard } from './-components/logged-card';
import { StatusCard } from './-components/status-card';
import { TimeEntryHeader } from './-components/time-entry-header';
import { useCopyLastWeek } from './-components/use-copy-last-week';
import { WeekGrid, type WeekGridHandle } from './-components/week-grid';

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
  const [isSubmitting, setIsSubmitting] = useState(false);
  const gridRef = useRef<WeekGridHandle>(null);

  const { handleCopyLastWeek, pendingCopyRows, applyPendingCopy, clearPendingCopy } = useCopyLastWeek({
    weekStart: weekData.weekStart,
    weekRows: weekData.rows,
    pickerOptions,
    gridRef,
  });

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
  const isSubmitted = weekData.isSubmitted;

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

  async function handleSubmitWeek() {
    if (!gridRef.current) return;
    const cells = gridRef.current.getCells();
    setIsSubmitting(true);
    try {
      await submitWeekFn({ data: { week: weekData.weekStart, cells } });
      gridRef.current?.resetDirty();
      setIsDirty(false);
      await navigate({ search: (previous) => previous });
      toast.success('Week submitted.');
    } catch {
      toast.error('Failed to submit week.');
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="flex flex-col gap-6 p-6">
      <TimeEntryHeader
        isSubmitted={isSubmitted}
        isDirty={isDirty}
        isSaving={isSaving}
        isSubmitting={isSubmitting}
        hasRows={hasRows}
        weekStart={weekData.weekStart}
        onSaveDraft={handleSaveDraft}
        onSubmitWeek={handleSubmitWeek}
      />

      <div className="grid grid-cols-1 gap-4 lg:grid-cols-[1fr_1.8fr_1.2fr]">
        <LoggedCard rows={weekData.rows} />
        <LastWeekCard summary={weekData.previousWeekSummary} isSubmitted={isSubmitted} onCopy={handleCopyLastWeek} />
        <StatusCard isSubmitted={isSubmitted} lastSavedAt={weekData.lastSavedAt ?? null} />
      </div>

      <CopyConfirmDialog open={pendingCopyRows !== null} onConfirm={applyPendingCopy} onCancel={clearPendingCopy} />

      <div key={weekData.weekStart} className="animate-fade-in">
        <WeekGrid
          ref={gridRef}
          weekStart={weekData.weekStart}
          savedRows={weekData.rows}
          pickerOptions={pickerOptions}
          isSubmitted={isSubmitted}
          onDirtyChange={setIsDirty}
        />
      </div>
    </div>
  );
}
