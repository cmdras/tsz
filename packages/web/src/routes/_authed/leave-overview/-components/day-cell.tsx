import { useNavigate } from '@tanstack/react-router';
import { getLeaveTypeColor } from '#/lib/leave-type-color';
import { cn } from '#/lib/utils';
import { fromIsoDateString, getIsoMonday, toIsoDateString } from '#/lib/date-utils';

interface DayCellProps {
  dateString: string;
  isToday: boolean;
  isWeekend: boolean;
  isInMonth: boolean;
  /** Leave type names (sorted alphabetically) for this day, undefined when day has no leave */
  typeNamesForDay: string[] | undefined;
  /** Name of the focused leave type, or undefined when no focus is active */
  focusedTypeName: string | undefined;
}

function isoMondayForDate(dateString: string): string {
  const date = fromIsoDateString(dateString);
  return toIsoDateString(getIsoMonday(date));
}

/**
 * Resolves the outline border class for a day cell.
 *
 * - No leave on this day → no outline
 * - No focus active → theme primary outline
 * - Focus active, focused type ∈ day's types → focused type's colour
 * - Focus active, focused type ∉ day's types → first alphabetical type's colour (multi-type fallback)
 */
function resolveOutlineClass(
  typeNamesForDay: string[] | undefined,
  focusedTypeName: string | undefined,
): string | null {
  if (!typeNamesForDay || typeNamesForDay.length === 0) return null;

  if (!focusedTypeName) {
    return 'border-2 border-primary';
  }

  const hasFocusedType = typeNamesForDay.includes(focusedTypeName);
  const activeTypeName = hasFocusedType ? focusedTypeName : typeNamesForDay[0];
  const colors = getLeaveTypeColor(activeTypeName);
  return `border-2 ${colors.outline}`;
}

export function DayCell({ dateString, isToday, isWeekend, isInMonth, typeNamesForDay, focusedTypeName }: DayCellProps) {
  const navigate = useNavigate();
  const weekParam = isoMondayForDate(dateString);
  const dayNumber = fromIsoDateString(dateString).getDate();
  const outlineClass = resolveOutlineClass(typeNamesForDay, focusedTypeName);

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
        outlineClass,
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
