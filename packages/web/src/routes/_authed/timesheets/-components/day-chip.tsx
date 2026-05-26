import { Badge } from '#/components/ui/badge';
import { getAvatarColor } from '#/lib/utils';
import type { MonthEntryResponse } from '#/features/timesheets/timesheets.server';

interface DayChipProps {
  entry: MonthEntryResponse;
}

export function DayChip({ entry }: DayChipProps) {
  if (entry.kind === 'task') {
    const customerName = entry.customerName ?? '';
    const label = `${customerName} · ${entry.hours}h`;
    const backgroundColor = getAvatarColor(customerName);

    return (
      <Badge
        className="max-w-full cursor-pointer truncate text-xs text-white"
        style={{ backgroundColor }}
        title={label}
      >
        {label}
      </Badge>
    );
  }

  const leaveLabel = `${entry.leaveTypeName ?? 'Leave'} · ${entry.hours}h`;

  return (
    <Badge className="max-w-full cursor-pointer truncate bg-amber-500 text-xs text-white" title={leaveLabel}>
      {leaveLabel}
    </Badge>
  );
}

interface OverflowChipProps {
  count: number;
}

export function OverflowChip({ count }: OverflowChipProps) {
  return (
    <Badge className="max-w-full cursor-pointer truncate text-xs" variant="outline">
      +{count} more
    </Badge>
  );
}
