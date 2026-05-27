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
    <li>
      <div className="flex items-center justify-between text-sm">
        <div className="flex min-w-0 items-center gap-1.5">
          <span className={`h-2 w-2 shrink-0 rounded-full ${colors.dot}`} aria-hidden />
          <span className="min-w-0 truncate">{name}</span>
        </div>
        <span className="ml-2 shrink-0 tabular-nums text-muted-foreground">{remaining}</span>
      </div>
      <div className="mt-1 h-0.5 overflow-hidden rounded-full bg-muted">
        <div className={`h-full rounded-full ${colors.dot}`} style={{ width: `${fillPercent}%` }} />
      </div>
    </li>
  );
}
