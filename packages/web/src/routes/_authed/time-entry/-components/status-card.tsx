import { Card, CardContent } from '#/components/ui/card';

function formatTime(isoString: string): string {
  const date = new Date(isoString);
  const hours = date.getHours().toString().padStart(2, '0');
  const minutes = date.getMinutes().toString().padStart(2, '0');
  return `${hours}:${minutes}`;
}

export function StatusCard({ isSubmitted, lastSavedAt }: { isSubmitted: boolean; lastSavedAt: string | null }) {
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
          {lastSavedAt ? `Saved at ${formatTime(lastSavedAt)}.` : 'Not saved yet.'}
        </span>
      </CardContent>
    </Card>
  );
}
