import { getIsoMonday, toIsoDateString } from '#/lib/date-utils';
import { DayCell } from './day-cell';

const COLUMN_HEADERS = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'] as const;

interface DayCellData {
  dateString: string;
  isInMonth: boolean;
}

function buildMonthDays(year: number, monthIndex: number): DayCellData[] {
  const firstDay = new Date(year, monthIndex, 1);
  const gridStart = getIsoMonday(firstDay);

  const lastDay = new Date(year, monthIndex + 1, 0);
  const lastDayOfWeek = lastDay.getDay();
  // Days until Sunday: Sun=0 → 0 extra; Mon=1 → 6 extra; …
  const daysUntilSunday = lastDayOfWeek === 0 ? 0 : 7 - lastDayOfWeek;
  const gridStartMs = gridStart.getTime();
  const gridEndMs = lastDay.getTime() + daysUntilSunday * 24 * 60 * 60 * 1000;
  const totalDays = Math.round((gridEndMs - gridStartMs) / (24 * 60 * 60 * 1000)) + 1;

  return Array.from({ length: totalDays }, (_, dayOffset) => {
    const date = new Date(gridStartMs + dayOffset * 24 * 60 * 60 * 1000);
    return {
      dateString: toIsoDateString(date),
      isInMonth: date.getMonth() === monthIndex,
    };
  });
}

interface MonthMiniProps {
  year: number;
  monthIndex: number;
  todayIso: string;
}

export function MonthMini({ year, monthIndex, todayIso }: MonthMiniProps) {
  const monthName = new Date(year, monthIndex, 1).toLocaleString('en-GB', { month: 'long' });
  const days = buildMonthDays(year, monthIndex);

  return (
    <div className="flex flex-col gap-1">
      <p className="text-sm font-medium text-center">{monthName}</p>
      <div className="grid grid-cols-7">
        {COLUMN_HEADERS.map((header) => (
          <div key={header} className="px-0.5 py-0.5 text-center text-xs font-medium text-muted-foreground">
            {header}
          </div>
        ))}
        {days.map((day) => {
          const dayOfWeek = new Date(day.dateString + 'T00:00:00').getDay();
          const isWeekend = dayOfWeek === 0 || dayOfWeek === 6;
          return (
            <DayCell
              key={day.dateString}
              dateString={day.dateString}
              isToday={day.dateString === todayIso}
              isWeekend={isWeekend}
              isInMonth={day.isInMonth}
            />
          );
        })}
      </div>
    </div>
  );
}
