import { createFileRoute, useBlocker } from '@tanstack/react-router';
import { useEffect, useRef, useState } from 'react';
import { toast } from 'sonner';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from '#/components/ui/alert-dialog';
import { Button } from '#/components/ui/button';
import { Card, CardContent } from '#/components/ui/card';
import { fetchPickerOptions, fetchWeek, saveDraft, submitWeekFn } from '#/features/time-entries/time-entries.functions';
import { weekSearchSchema } from '#/features/time-entries/time-entries.schemas';
import type { WeekRowResponse } from '#/features/time-entries/time-entries.server';
import { addDays, fromIsoDateString, getIsoMonday, toIsoDateString } from '#/lib/date-utils';
import { WeekGrid, type WeekGridHandle } from './-components/week-grid';
import { LastWeekCard } from './-components/last-week-card';
import { LoggedCard } from './-components/logged-card';
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

function formatTime(isoString: string): string {
  const date = new Date(isoString);
  const hours = date.getHours().toString().padStart(2, '0');
  const minutes = date.getMinutes().toString().padStart(2, '0');
  return `${hours}:${minutes}`;
}

function StatusCard({ isSubmitted, lastSavedAt }: { isSubmitted: boolean; lastSavedAt: string | null }) {
  return (
    <Card className="relative h-full py-4">
      <span
        aria-hidden
        className="pointer-events-none absolute left-1.5 top-1.5 h-3 w-3 border-l-2 border-t-2 border-primary"
      />
      <span
        aria-hidden
        className="pointer-events-none absolute right-1.5 top-1.5 h-3 w-3 border-r-2 border-t-2 border-primary"
      />
      <span
        aria-hidden
        className="pointer-events-none absolute bottom-1.5 left-1.5 h-3 w-3 border-b-2 border-l-2 border-primary"
      />
      <span
        aria-hidden
        className="pointer-events-none absolute bottom-1.5 right-1.5 h-3 w-3 border-b-2 border-r-2 border-primary"
      />
      <CardContent className="flex h-full flex-col gap-2">
        <span className="text-xs font-medium uppercase tracking-wider text-primary">Status</span>
        <span className="text-lg font-semibold">{isSubmitted ? 'Submitted.' : 'Draft — not submitted.'}</span>
        <span className="text-xs text-muted-foreground">
          {lastSavedAt ? `Auto-saved at ${formatTime(lastSavedAt)}.` : 'Not saved yet.'}
        </span>
      </CardContent>
    </Card>
  );
}

function TimeEntryPage() {
  const { weekData, pickerOptions } = Route.useLoaderData();
  const navigate = Route.useNavigate();
  const [isDirty, setIsDirty] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [pendingCopyRows, setPendingCopyRows] = useState<WeekRowResponse[] | null>(null);
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

  async function handleCopyLastWeek() {
    const previousWeekStart = toIsoDateString(addDays(fromIsoDateString(weekData.weekStart), -7));
    let previousWeekData;
    try {
      previousWeekData = await fetchWeek({ data: { week: previousWeekStart } });
    } catch {
      toast.error('Failed to fetch last week.');
      return;
    }

    if (previousWeekData.rows.length === 0) {
      toast.info('No entries last week.');
      return;
    }

    const validTaskIds = new Set([
      ...weekData.rows.filter((row) => row.contractTaskId).map((row) => row.contractTaskId!),
      ...pickerOptions.availableTasks.map((task) => task.contractTaskId),
    ]);
    const validLeaveTypeIds = new Set([
      ...weekData.rows.filter((row) => row.leaveTypeId).map((row) => row.leaveTypeId!),
      ...pickerOptions.availableLeaveTypes.map((leaveType) => leaveType.leaveTypeId),
    ]);

    const filteredRows = previousWeekData.rows.filter((row) => {
      if (row.contractTaskId) return validTaskIds.has(row.contractTaskId);
      if (row.leaveTypeId) return validLeaveTypeIds.has(row.leaveTypeId);
      return false;
    });

    if (filteredRows.length === 0) {
      toast.info('No entries last week.');
      return;
    }

    if (gridRef.current?.hasRows()) {
      setPendingCopyRows(filteredRows);
    } else {
      gridRef.current?.loadWeek(filteredRows);
    }
  }

  function applyPendingCopy() {
    if (pendingCopyRows && gridRef.current) {
      gridRef.current.loadWeek(pendingCopyRows);
    }
    setPendingCopyRows(null);
  }

  function getPageTitleSuffix() {
    if (isSubmitted) return 'submitted.';
    return hasRows ? 'logged.' : 'empty.';
  }

  return (
    <div className="flex flex-col gap-6 p-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold">
          Time entry <em className="font-normal text-primary">{getPageTitleSuffix()}</em>
        </h1>
        <div className="flex items-center gap-3">
          {!isSubmitted && isDirty && (
            <Button onClick={handleSaveDraft} disabled={isSaving} size="sm" variant="outline">
              {isSaving ? 'Saving…' : 'Save draft'}
            </Button>
          )}
          {!isSubmitted && (
            <AlertDialog>
              <AlertDialogTrigger asChild>
                <Button size="sm" disabled={isSubmitting}>
                  Submit week
                </Button>
              </AlertDialogTrigger>
              <AlertDialogContent>
                <AlertDialogHeader>
                  <AlertDialogTitle>Submit this week?</AlertDialogTitle>
                  <AlertDialogDescription>
                    This will lock the week as final. You will not be able to make further changes.
                  </AlertDialogDescription>
                </AlertDialogHeader>
                <AlertDialogFooter>
                  <AlertDialogCancel>Cancel</AlertDialogCancel>
                  <AlertDialogAction onClick={handleSubmitWeek} disabled={isSubmitting}>
                    {isSubmitting ? 'Submitting…' : 'Submit'}
                  </AlertDialogAction>
                </AlertDialogFooter>
              </AlertDialogContent>
            </AlertDialog>
          )}
          <WeekNav weekStart={weekData.weekStart} />
        </div>
      </div>

      <div className="grid grid-cols-1 gap-4 lg:grid-cols-[1fr_1.8fr_1.2fr]">
        <LoggedCard rows={weekData.rows} />
        <LastWeekCard summary={weekData.previousWeekSummary} isSubmitted={isSubmitted} onCopy={handleCopyLastWeek} />
        <StatusCard isSubmitted={isSubmitted} lastSavedAt={weekData.lastSavedAt ?? null} />
      </div>

      <AlertDialog
        open={pendingCopyRows !== null}
        onOpenChange={(open) => {
          if (!open) setPendingCopyRows(null);
        }}
      >
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Overwrite this week?</AlertDialogTitle>
            <AlertDialogDescription>
              This will replace this week&apos;s hours with last week&apos;s entries.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction onClick={applyPendingCopy}>Overwrite</AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

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
