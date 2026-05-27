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
  const daysUntilSunday = lastDayOfWeek === 0 ? 0 : 7 - lastDayOfWeek;
  const gridEnd = new Date(lastDay.getFullYear(), lastDay.getMonth(), lastDay.getDate() + daysUntilSunday);

  const days: DayCellData[] = [];
  let cursor = new Date(gridStart.getFullYear(), gridStart.getMonth(), gridStart.getDate());
  while (cursor <= gridEnd) {
    days.push({
      dateString: toIsoDateString(cursor),
      isInMonth: cursor.getMonth() === monthIndex,
    });
    cursor = new Date(cursor.getFullYear(), cursor.getMonth(), cursor.getDate() + 1);
  }
  return days;
}

interface MonthMiniProps {
  year: number;
  monthIndex: number;
  todayIso: string;
  /** Map from ISO date string → sorted leave type names for that day */
  dayMap: Map<string, string[]>;
  /** Name of the focused leave type, or undefined when no focus is active */
  focusedTypeName: string | undefined;
}

export function MonthMini({ year, monthIndex, todayIso, dayMap, focusedTypeName }: MonthMiniProps) {
  const monthName = new Date(year, monthIndex, 1).toLocaleString('en-GB', { month: 'long' });
  const days = buildMonthDays(year, monthIndex);

  return (
    <div className="flex flex-col overflow-hidden rounded-lg border">
      <div className="border-b px-2 py-1 text-center text-sm font-medium">{monthName}</div>
      <div className="grid grid-cols-7">
        {COLUMN_HEADERS.map((header) => (
          <div key={header} className="px-0.5 py-0.5 text-center text-xs font-medium text-muted-foreground">
            {header}
          </div>
        ))}
        {days.map((day) => {
          const dayOfWeek = new Date(day.dateString + 'T00:00:00').getDay();
          const isWeekend = dayOfWeek === 0 || dayOfWeek === 6;
          const typeNamesForDay = dayMap.get(day.dateString);
          return (
            <DayCell
              key={day.dateString}
              dateString={day.dateString}
              isToday={day.dateString === todayIso}
              isWeekend={isWeekend}
              isInMonth={day.isInMonth}
              typeNamesForDay={typeNamesForDay}
              focusedTypeName={focusedTypeName}
            />
          );
        })}
      </div>
    </div>
  );
}
