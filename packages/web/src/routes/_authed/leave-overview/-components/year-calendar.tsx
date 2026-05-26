import type { LeaveOverviewDayItem, LeaveOverviewTypeItem } from '#/features/leave-overview/leave-overview.schemas';
import { toIsoDateString } from '#/lib/date-utils';
import { MonthMini } from './month-mini';
import { LegendChips } from './legend-chips';

const MONTH_INDICES = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11] as const;

interface YearCalendarProps {
  year: number;
  types: LeaveOverviewTypeItem[];
  days: LeaveOverviewDayItem[];
  focus: string | undefined;
}

export function YearCalendar({ year, types, days, focus }: YearCalendarProps) {
  const todayIso = toIsoDateString(new Date());

  // Map from dateString → sorted leave type names for that day (for fallback color resolution)
  const dayMap = new Map<string, string[]>();
  const typeNameMap = new Map<string, string>();
  for (const leaveType of types) {
    typeNameMap.set(leaveType.id, leaveType.name);
  }
  for (const day of days) {
    // Sort type names alphabetically for multi-type fallback
    const sortedNames = day.leaveTypeIds
      .map((id) => typeNameMap.get(id) ?? '')
      .filter(Boolean)
      .toSorted((a, b) => a.localeCompare(b));
    dayMap.set(day.date, sortedNames);
  }

  const focusedType = focus ? types.find((leaveType) => leaveType.id === focus) : undefined;

  return (
    <div className="flex min-w-0 flex-1 flex-col gap-4">
      <div className="flex flex-col gap-2">
        {focusedType && (
          <p className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">
            FOCUSED ON {focusedType.name.toUpperCase()}
          </p>
        )}
        <LegendChips types={types} focus={focus} />
      </div>
      <div className="grid grid-cols-4 gap-6">
        {MONTH_INDICES.map((monthIndex) => (
          <MonthMini
            key={monthIndex}
            year={year}
            monthIndex={monthIndex}
            todayIso={todayIso}
            dayMap={dayMap}
            focusedTypeName={focusedType?.name}
          />
        ))}
      </div>
    </div>
  );
}
