import { useNavigate } from '@tanstack/react-router';
import { cn } from '#/lib/utils';
import { fromIsoDateString, getIsoMonday, toIsoDateString } from '#/lib/date-utils';
import type { MonthDayResponse } from '#/features/timesheets/timesheets.server';
import { DayChip, OverflowChip } from './day-chip';

const MAX_VISIBLE_CHIPS = 4;

interface DayCellProps {
  day: MonthDayResponse;
  isToday: boolean;
  isWeekend: boolean;
}

function isoMondayForDate(dateString: string): string {
  const date = fromIsoDateString(dateString);
  return toIsoDateString(getIsoMonday(date));
}

export function DayCell({ day, isToday, isWeekend }: DayCellProps) {
  const navigate = useNavigate();
  const weekParam = isoMondayForDate(day.date);

  const sortedEntries = [...day.entries].toSorted((a, b) => b.hours - a.hours);
  const visibleEntries = sortedEntries.slice(0, MAX_VISIBLE_CHIPS);
  const overflowCount = sortedEntries.length - MAX_VISIBLE_CHIPS;

  function handleCellClick() {
    void navigate({ to: '/time-entry', search: { week: weekParam } });
  }

  return (
    <div
      role="button"
      tabIndex={0}
      onClick={handleCellClick}
      onKeyDown={(keyboardEvent) => {
        if (keyboardEvent.key === 'Enter' || keyboardEvent.key === ' ') handleCellClick();
      }}
      className={cn(
        'cursor-pointer border-b border-r p-1',
        'focus:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-1',
        isWeekend && 'bg-muted/30',
        !day.isInMonth && 'opacity-40',
      )}
    >
      <div className="flex items-start justify-between">
        <span
          className={cn(
            'inline-flex h-6 w-6 items-center justify-center rounded-full text-sm',
            isToday && 'bg-primary font-semibold text-primary-foreground',
          )}
        >
          {new Date(day.date + 'T00:00:00').getDate()}
        </span>
        {day.totalHours > 0 && <span className="text-xs text-muted-foreground">{day.totalHours}h</span>}
      </div>

      {visibleEntries.length > 0 && (
        <div className="mt-1 flex flex-col gap-0.5">
          {visibleEntries.map((entry) => (
            <DayChip key={entry.id} entry={entry} />
          ))}
          {overflowCount > 0 && <OverflowChip count={overflowCount} />}
        </div>
      )}
    </div>
  );
}
