import { addDays, fromIsoDateString, DAYS_OF_WEEK } from '#/lib/date-utils';

const WEEKEND_INDICES = new Set([5, 6]);

interface WeekGridProps {
  weekStart: string;
}

export function WeekGrid({ weekStart }: WeekGridProps) {
  const monday = fromIsoDateString(weekStart);

  return (
    <div className="rounded-lg border">
      <div className="grid grid-cols-[auto_repeat(7,1fr)] border-b">
        <div className="p-3" />
        {DAYS_OF_WEEK.map((day, index) => {
          const date = addDays(monday, index);
          const isWeekend = WEEKEND_INDICES.has(index);
          return (
            <div
              key={day}
              className={`border-l p-3 text-center text-sm font-medium ${isWeekend ? 'bg-muted/40 text-muted-foreground' : ''}`}
            >
              <div>{day}</div>
              <div className="text-xs text-muted-foreground">
                {date.toLocaleDateString('en-GB', { day: 'numeric', month: 'short' })}
              </div>
            </div>
          );
        })}
      </div>

      <div className="p-8 text-center text-sm text-muted-foreground">
        No tasks yet — add a task or leave to get started
      </div>
    </div>
  );
}
