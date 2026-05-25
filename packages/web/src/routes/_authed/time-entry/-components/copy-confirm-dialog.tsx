import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '#/components/ui/alert-dialog';

interface CopyConfirmDialogProps {
  open: boolean;
  onConfirm: () => void;
  onCancel: () => void;
}

export function CopyConfirmDialog({ open, onConfirm, onCancel }: CopyConfirmDialogProps) {
  return (
    <AlertDialog
      open={open}
      onOpenChange={(isOpen) => {
        if (!isOpen) onCancel();
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
          <AlertDialogAction onClick={onConfirm}>Overwrite</AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
