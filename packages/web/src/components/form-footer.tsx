import { Button } from '#/components/ui/button';

interface FormFooterProps {
  canSubmit: boolean;
  isPending: boolean;
  onCancel: () => void;
  submitLabel?: string;
}

export function FormFooter({ canSubmit, isPending, onCancel, submitLabel = 'Save' }: FormFooterProps) {
  return (
    <div className="flex gap-2">
      <Button type="submit" disabled={!canSubmit || isPending}>
        {isPending ? 'Saving…' : submitLabel}
      </Button>
      <Button type="button" variant="outline" onClick={onCancel} disabled={isPending}>
        Cancel
      </Button>
    </div>
  );
}
