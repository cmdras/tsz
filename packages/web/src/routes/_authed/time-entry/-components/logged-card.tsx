import { Card, CardContent } from '#/components/ui/card';
import { Progress } from '#/components/ui/progress';
import type { WeekRowResponse } from '#/features/time-entries/time-entries.server';

const TARGET_HOURS = 40;

function roundHours(hours: number): number {
  return Math.round(hours * 100) / 100;
}

function formatDelta(value: number): string {
  if (value === 0) return 'on target';
  const sign = value > 0 ? '+' : '−';
  return `${sign}${Math.abs(value)}h`;
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
    <Card>
      <CardContent className="flex flex-col gap-2 py-3">
        <div className="flex items-center justify-between text-sm">
          <span className="font-medium">Logged this week</span>
          <span className="text-muted-foreground">
            {totalHours}h / {TARGET_HOURS}h{' '}
            <span className={delta < 0 ? 'text-destructive' : 'text-green-600'}>{formatDelta(delta)}</span>
          </span>
        </div>
        <Progress value={progressPercent} className="h-2" />
      </CardContent>
    </Card>
  );
}
