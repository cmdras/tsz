import { useNavigate } from '@tanstack/react-router';
import { cn } from '#/lib/utils';
import { fromIsoDateString, getIsoMonday, toIsoDateString } from '#/lib/date-utils';

interface DayCellProps {
  dateString: string;
  isToday: boolean;
  isWeekend: boolean;
  isInMonth: boolean;
}

function isoMondayForDate(dateString: string): string {
  const date = fromIsoDateString(dateString);
  return toIsoDateString(getIsoMonday(date));
}

export function DayCell({ dateString, isToday, isWeekend, isInMonth }: DayCellProps) {
  const navigate = useNavigate();
  const weekParam = isoMondayForDate(dateString);
  const dayNumber = fromIsoDateString(dateString).getDate();

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
        'cursor-pointer p-0.5',
        'focus:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-1',
        isWeekend && 'bg-muted/30',
        !isInMonth && 'opacity-40',
      )}
    >
      <span
        className={cn(
          'inline-flex h-6 w-6 items-center justify-center rounded-full text-xs',
          isToday && 'bg-primary font-semibold text-primary-foreground',
        )}
      >
        {dayNumber}
      </span>
    </div>
  );
}
