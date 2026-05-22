import { XIcon } from 'lucide-react';
import { cn, getAvatarColor } from '#/lib/utils';
import type { LeaveGridRow, TaskGridRow } from './week-grid-model';

export function TaskRowLabel({ row }: { row: TaskGridRow }) {
  const initials = row.customerName.slice(0, 2).toUpperCase();
  const avatarColor = getAvatarColor(row.customerName);

  return (
    <div className="flex items-center gap-2 p-3">
      <span
        className="flex h-7 w-7 shrink-0 items-center justify-center rounded-full text-xs font-semibold text-white"
        style={{ backgroundColor: avatarColor }}
      >
        {initials}
      </span>
      <div className="min-w-0">
        <div className="truncate font-semibold text-sm">{row.customerName}</div>
        <div className="truncate text-xs text-muted-foreground">
          {row.contractSubject} · {row.taskName}
        </div>
      </div>
    </div>
  );
}

export function LeaveRowLabel({ row }: { row: LeaveGridRow }) {
  const initials = row.leaveTypeName.slice(0, 2).toUpperCase();

  return (
    <div className="flex items-center gap-2 p-3">
      <span className="flex h-7 w-7 shrink-0 items-center justify-center rounded-full bg-amber-500 text-xs font-semibold text-white">
        {initials}
      </span>
      <div className="min-w-0">
        <div className="truncate font-semibold text-sm">{row.leaveTypeName}</div>
      </div>
    </div>
  );
}

export function DisabledCell({ isWeekend }: { isWeekend: boolean }) {
  return (
    <div className={cn('border-l p-2', isWeekend && 'bg-muted/40')}>
      <div className="relative flex h-8 items-center justify-center rounded border border-dashed border-muted-foreground/20 bg-muted/20">
        {isWeekend && <XIcon className="h-4 w-4 text-primary/40" />}
      </div>
    </div>
  );
}
