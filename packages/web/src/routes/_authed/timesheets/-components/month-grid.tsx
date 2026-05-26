import { cn } from '#/lib/utils';
import type { MonthDayResponse } from '#/features/timesheets/timesheets.server';

interface MonthGridProps {
  days: MonthDayResponse[];
  today: string;
}

const DAY_HEADERS = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'] as const;

function isWeekend(dateString: string): boolean {
  const date = new Date(dateString + 'T00:00:00');
  const dayOfWeek = date.getDay();
  return dayOfWeek === 0 || dayOfWeek === 6;
}

export function MonthGrid({ days, today }: MonthGridProps) {
  return (
    <div className="overflow-hidden rounded-lg border">
      <div className="grid grid-cols-7">
        {DAY_HEADERS.map((header) => (
          <div
            key={header}
            className={cn(
              'border-b px-2 py-1 text-center text-sm font-medium text-muted-foreground',
              (header === 'Sat' || header === 'Sun') && 'bg-muted/50',
            )}
          >
            {header}
          </div>
        ))}

        {days.map((day) => {
          const isToday = day.date === today;
          const weekend = isWeekend(day.date);

          return (
            <div
              key={day.date}
              className={cn('min-h-20 border-b border-r p-1', weekend && 'bg-muted/30', !day.isInMonth && 'opacity-40')}
            >
              <span
                className={cn(
                  'inline-flex h-6 w-6 items-center justify-center rounded-full text-sm',
                  isToday && 'bg-primary font-semibold text-primary-foreground',
                )}
              >
                {new Date(day.date + 'T00:00:00').getDate()}
              </span>
            </div>
          );
        })}
      </div>
    </div>
  );
}
