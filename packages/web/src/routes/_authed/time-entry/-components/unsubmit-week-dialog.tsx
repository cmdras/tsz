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

interface UnsubmitWeekDialogProps {
  isUnsubmitting: boolean;
  onUnsubmit: () => void;
}

export function UnsubmitWeekDialog({ isUnsubmitting, onUnsubmit }: UnsubmitWeekDialogProps) {
  return (
    <AlertDialog>
      <AlertDialogTrigger asChild>
        <Button size="sm" variant="outline" disabled={isUnsubmitting}>
          Unsubmit week
        </Button>
      </AlertDialogTrigger>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>Reopen this week for editing?</AlertDialogTitle>
          <AlertDialogDescription>
            This will remove the submission and reopen the week so it can be edited again. Any existing entries will
            remain intact.
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel>Cancel</AlertDialogCancel>
          <AlertDialogAction onClick={onUnsubmit} disabled={isUnsubmitting}>
            {isUnsubmitting ? 'Reopening…' : 'Reopen week'}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
