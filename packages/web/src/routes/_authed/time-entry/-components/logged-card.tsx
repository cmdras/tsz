import { Card, CardContent } from '#/components/ui/card';
import { Progress } from '#/components/ui/progress';
import type { WeekRowResponse } from '#/features/time-entries/time-entries.server';

const TARGET_HOURS = 40;

function roundHours(hours: number): number {
  return Math.round(hours * 100) / 100;
}

function formatDelta(value: number): string {
  if (value === 0) return 'on target';
  if (value < 0) return `${Math.abs(value)}h to target`;
  return `+${value}h over target`;
}

interface LoggedCardProps {
  rows: WeekRowResponse[];
}

export function LoggedCard({ rows }: LoggedCardProps) {
  const totalHours = roundHours(
    rows.reduce((sum, row) => sum + row.hours.reduce((rowSum, hour) => rowSum + (hour ?? 0), 0), 0),
  );
  const delta = roundHours(totalHours - TARGET_HOURS);
  const progressPercent = Math.min((totalHours / TARGET_HOURS) * 100, 100);

  return (
    <Card className="h-full py-4">
      <CardContent className="flex h-full flex-col gap-3">
        <span className="text-xs font-medium uppercase tracking-wider text-muted-foreground">Logged this week</span>
        <div className="flex items-baseline gap-1.5">
          <span className="text-4xl font-bold text-primary leading-none">{totalHours}h</span>
          <span className="text-sm text-muted-foreground">/ {TARGET_HOURS}h</span>
        </div>
        <Progress value={progressPercent} className="h-1.5" />
        <span className={`text-xs ${delta < 0 ? 'text-muted-foreground' : 'text-primary'}`}>{formatDelta(delta)}</span>
      </CardContent>
    </Card>
  );
}
