import { useNavigate } from '@tanstack/react-router';
import type { LeaveOverviewTypeItem } from '#/features/leave-overview/leave-overview.schemas';
import { getLeaveTypeColor } from '#/lib/leave-type-color';
import { cn } from '#/lib/utils';

interface LegendChipsProps {
  types: LeaveOverviewTypeItem[];
  focus: string | undefined;
}

export function LegendChips({ types, focus }: LegendChipsProps) {
  const navigate = useNavigate({ from: '/leave-overview/' });

  const sortedTypes = types.toSorted((a, b) => a.name.localeCompare(b.name));

  function handleChipClick(typeId: string) {
    const nextFocus = focus === typeId ? undefined : typeId;
    void navigate({
      search: (previous) => ({ ...previous, focus: nextFocus }),
    });
  }

  if (sortedTypes.length === 0) return null;

  return (
    <div className="flex flex-wrap gap-2">
      {sortedTypes.map((leaveType) => {
        const colors = getLeaveTypeColor(leaveType.name);
        const isFocused = focus === leaveType.id;
        return (
          <button
            key={leaveType.id}
            type="button"
            onClick={() => handleChipClick(leaveType.id)}
            className={cn(
              'inline-flex items-center gap-1.5 rounded-full border px-3 py-1 text-xs font-medium transition-colors',
              'focus:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-1',
              isFocused
                ? 'bg-foreground text-background border-foreground'
                : 'bg-background text-foreground border-border hover:bg-muted',
            )}
          >
            <span className={cn('h-2 w-2 shrink-0 rounded-full', colors.dot)} aria-hidden />
            {leaveType.name}
          </button>
        );
      })}
    </div>
  );
}
