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

interface SubmitWeekDialogProps {
  isSubmitting: boolean;
  onSubmit: () => void;
}

export function SubmitWeekDialog({ isSubmitting, onSubmit }: SubmitWeekDialogProps) {
  return (
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
          <AlertDialogAction onClick={onSubmit} disabled={isSubmitting}>
            {isSubmitting ? 'Submitting…' : 'Submit'}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
