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

export function LastWeekCard({ summary, isSubmitted, onCopy }: LastWeekCardProps) {
  const hasChips = summary.chips.length > 0 || summary.overflow !== null;

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
        <div className="flex flex-wrap items-center gap-2">
          {hasChips ? (
            <>
              {summary.chips.map((chip) => (
                <Badge key={chip.label} variant="secondary" className="gap-1.5 text-xs">
                  <span className="h-1.5 w-1.5 rounded-full bg-primary" />
                  {chip.label} · {chip.hours}h
                </Badge>
              ))}
              {summary.overflow && <span className="text-xs text-muted-foreground">{summary.overflow}</span>}
            </>
          ) : (
            <span className="text-sm text-muted-foreground">No entries</span>
          )}
        </div>
      </CardContent>
    </Card>
  );
}
