import { RotateCcwIcon } from 'lucide-react';
import { Badge } from '#/components/ui/badge';
import { Button } from '#/components/ui/button';
import { Card, CardContent } from '#/components/ui/card';
import type { WeekPreviousSummaryResponse } from '#/features/time-entries/time-entries.server';

interface LastWeekCardProps {
  summary: WeekPreviousSummaryResponse;
  isSubmitted: boolean;
  onCopy: () => void;
}

function OverflowIndicator({ overflow }: { overflow: string | null }) {
  if (!overflow) return null;
  return <span className="text-xs text-muted-foreground">{overflow}</span>;
}

function ChipSummary({ chips, overflow }: { chips: WeekPreviousSummaryResponse['chips']; overflow: string | null }) {
  const hasEntries = chips.length > 0 || overflow !== null;
  if (!hasEntries) return <span className="text-sm text-muted-foreground">No entries</span>;
  return (
    <div className="flex flex-wrap items-center gap-2">
      {chips.map((chip) => (
        <Badge key={chip.label} variant="secondary" className="gap-1.5 text-xs">
          <span className="h-1.5 w-1.5 rounded-full bg-primary" />
          {chip.label} · {chip.hours}h
        </Badge>
      ))}
      <OverflowIndicator overflow={overflow} />
    </div>
  );
}

export function LastWeekCard({ summary, isSubmitted, onCopy }: LastWeekCardProps) {
  return (
    <Card className="h-full py-4">
      <CardContent className="flex h-full flex-col gap-3">
        <div className="flex items-center justify-between gap-3">
          <span className="text-xs font-medium uppercase tracking-wider text-muted-foreground">Last week</span>
          {!isSubmitted && (
            <Button variant="outline" size="sm" onClick={onCopy} className="gap-1.5">
              <RotateCcwIcon className="h-3.5 w-3.5" />
              Copy last week
            </Button>
          )}
        </div>
        <ChipSummary chips={summary.chips} overflow={summary.overflow} />
      </CardContent>
    </Card>
  );
}
