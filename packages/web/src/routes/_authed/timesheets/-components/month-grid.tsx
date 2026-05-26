import type { MonthDayResponse } from '#/features/timesheets/timesheets.server';
import { DayCell } from './day-cell';

interface MonthGridProps {
  days: MonthDayResponse[];
  today: string;
}

const DAY_HEADERS = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'] as const;

function isWeekendDate(dateString: string): boolean {
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
            className={`border-b px-2 py-1 text-center text-sm font-medium text-muted-foreground${header === 'Sat' || header === 'Sun' ? ' bg-muted/50' : ''}`}
          >
            {header}
          </div>
        ))}

        {days.map((day) => (
          <DayCell key={day.date} day={day} isToday={day.date === today} isWeekend={isWeekendDate(day.date)} />
        ))}
      </div>
    </div>
  );
}
