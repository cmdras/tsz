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
import { Badge } from '#/components/ui/badge';
import { Button } from '#/components/ui/button';
import { Card, CardContent } from '#/components/ui/card';
import { fetchPickerOptions, fetchWeek, saveDraft, submitWeekFn } from '#/features/time-entries/time-entries.functions';
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

function formatTime(isoString: string): string {
  const date = new Date(isoString);
  const hours = date.getHours().toString().padStart(2, '0');
  const minutes = date.getMinutes().toString().padStart(2, '0');
  return `${hours}:${minutes}`;
}

function StatusCard({ isSubmitted, lastSavedAt }: { isSubmitted: boolean; lastSavedAt: string | null }) {
  return (
    <Card>
      <CardContent className="flex items-center gap-3 py-3">
        <Badge variant={isSubmitted ? 'default' : 'secondary'}>{isSubmitted ? 'Submitted' : 'Draft'}</Badge>
        <span className="text-sm text-muted-foreground">
          {lastSavedAt ? `Last saved at ${formatTime(lastSavedAt)}` : 'Not saved yet'}
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

      <StatusCard isSubmitted={isSubmitted} lastSavedAt={weekData.lastSavedAt ?? null} />

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
