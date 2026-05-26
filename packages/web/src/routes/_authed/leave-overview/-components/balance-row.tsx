import { getLeaveTypeColor } from '#/lib/leave-type-color';
import type { LeaveOverviewTypeItem } from '#/features/leave-overview/leave-overview.schemas';

interface BalanceRowProps {
  leaveType: LeaveOverviewTypeItem;
}

export function BalanceRow({ leaveType }: BalanceRowProps) {
  const { name, allowance, takenDays } = leaveType;
  const remaining = Math.max(0, allowance - takenDays);
  const fillRatio = allowance > 0 ? Math.min(1, Math.max(0, takenDays / allowance)) : 0;
  const fillPercent = fillRatio * 100;
  const colors = getLeaveTypeColor(name);

  return (
    <div className="flex items-center gap-3">
      <span className={`h-2.5 w-2.5 shrink-0 rounded-full ${colors.dot}`} aria-hidden />
      <div className="min-w-0 flex-1">
        <div className="mb-1 flex items-baseline justify-between gap-2">
          <span className="truncate text-sm font-medium">{name}</span>
          <span className="shrink-0 text-xs text-muted-foreground">
            {remaining} / {allowance} LEFT
          </span>
        </div>
        <div className="h-1.5 w-full overflow-hidden rounded-full bg-muted">
          <div className="h-full rounded-full bg-foreground/60 transition-all" style={{ width: `${fillPercent}%` }} />
        </div>
      </div>
      <span className="shrink-0 text-2xl font-bold tabular-nums">{remaining}</span>
    </div>
  );
}
