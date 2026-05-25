import { Button } from '#/components/ui/button';
import { SubmitWeekDialog } from './submit-week-dialog';
import { WeekNav } from './week-nav';

interface TimeEntryHeaderProps {
  isSubmitted: boolean;
  isDirty: boolean;
  isSaving: boolean;
  isSubmitting: boolean;
  hasRows: boolean;
  weekStart: string;
  onSaveDraft: () => void;
  onSubmitWeek: () => void;
}

export function TimeEntryHeader({
  isSubmitted,
  isDirty,
  isSaving,
  isSubmitting,
  hasRows,
  weekStart,
  onSaveDraft,
  onSubmitWeek,
}: TimeEntryHeaderProps) {
  const titleSuffix = isSubmitted ? 'submitted.' : hasRows ? 'logged.' : 'empty.';

  return (
    <div className="flex items-center justify-between">
      <h1 className="text-2xl font-semibold">
        Time entry <em className="font-normal text-primary">{titleSuffix}</em>
      </h1>
      <div className="flex items-center gap-3">
        {!isSubmitted && isDirty && (
          <Button onClick={onSaveDraft} disabled={isSaving} size="sm" variant="outline">
            {isSaving ? 'Saving…' : 'Save draft'}
          </Button>
        )}
        {!isSubmitted && <SubmitWeekDialog isSubmitting={isSubmitting} onSubmit={onSubmitWeek} />}
        <WeekNav weekStart={weekStart} />
      </div>
    </div>
  );
}
