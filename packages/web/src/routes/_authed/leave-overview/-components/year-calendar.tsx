import { toIsoDateString } from '#/lib/date-utils';
import { MonthMini } from './month-mini';

const MONTH_INDICES = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11] as const;

interface YearCalendarProps {
  year: number;
}

export function YearCalendar({ year }: YearCalendarProps) {
  const todayIso = toIsoDateString(new Date());

  return (
    <div className="grid grid-cols-4 gap-6 min-w-0 flex-1">
      {MONTH_INDICES.map((monthIndex) => (
        <MonthMini key={monthIndex} year={year} monthIndex={monthIndex} todayIso={todayIso} />
      ))}
    </div>
  );
}
