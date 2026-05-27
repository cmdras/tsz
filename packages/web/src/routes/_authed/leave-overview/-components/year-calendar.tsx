import type { LeaveOverviewDayItem, LeaveOverviewTypeItem } from '#/features/leave-overview/leave-overview.schemas';
import { toIsoDateString } from '#/lib/date-utils';
import { MonthMini } from './month-mini';

const MONTH_INDICES = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11] as const;

interface YearCalendarProps {
  year: number;
  types: LeaveOverviewTypeItem[];
  days: LeaveOverviewDayItem[];
  focusedTypeName: string | undefined;
}

export function YearCalendar({ year, types, days, focusedTypeName }: YearCalendarProps) {
  const todayIso = toIsoDateString(new Date());

  const dayMap = new Map<string, string[]>();
  const typeNameMap = new Map<string, string>();
  for (const leaveType of types) {
    typeNameMap.set(leaveType.id, leaveType.name);
  }
  for (const day of days) {
    const sortedNames = day.leaveTypeIds
      .map((id) => typeNameMap.get(id) ?? '')
      .filter(Boolean)
      .toSorted((a, b) => a.localeCompare(b));
    dayMap.set(day.date, sortedNames);
  }

  return (
    <div className="grid min-w-0 flex-1 grid-cols-4 gap-6">
      {MONTH_INDICES.map((monthIndex) => (
        <MonthMini
          key={monthIndex}
          year={year}
          monthIndex={monthIndex}
          todayIso={todayIso}
          dayMap={dayMap}
          focusedTypeName={focusedTypeName}
        />
      ))}
    </div>
  );
}
