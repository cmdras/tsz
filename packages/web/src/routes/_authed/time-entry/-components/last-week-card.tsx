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
    <Card>
      <CardContent className="flex items-center justify-between gap-3 py-3">
        <div className="flex flex-wrap items-center gap-2">
          <span className="text-sm font-medium">Last week</span>
          {hasChips ? (
            <>
              {summary.chips.map((chip) => (
                <Badge key={chip.label} variant="secondary" className="text-xs">
                  {chip.label} · {chip.hours}h
                </Badge>
              ))}
              {summary.overflow && <span className="text-xs text-muted-foreground">{summary.overflow}</span>}
            </>
          ) : (
            <span className="text-sm text-muted-foreground">No entries</span>
          )}
        </div>
        {!isSubmitted && (
          <Button variant="outline" size="sm" onClick={onCopy}>
            Copy last week
          </Button>
        )}
      </CardContent>
    </Card>
  );
}
