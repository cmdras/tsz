import type { LeaveOverviewTypeItem } from '#/features/leave-overview/leave-overview.schemas';
import { BalanceRow } from './balance-row';

interface BalanceSidebarProps {
  year: number;
  types: LeaveOverviewTypeItem[];
}

export function BalanceSidebar({ year, types }: BalanceSidebarProps) {
  const limitedTypes = types
    .filter((leaveType) => leaveType.mode === 'Limited')
    .toSorted((a, b) => a.name.localeCompare(b.name));

  const totalDaysLeft = limitedTypes.reduce(
    (total, leaveType) => total + Math.max(0, leaveType.allowance - leaveType.takenDays),
    0,
  );
  const totalDaysTaken = limitedTypes.reduce((total, leaveType) => total + leaveType.takenDays, 0);
  const typeCount = limitedTypes.length;

  return (
    <aside className="flex w-64 shrink-0 flex-col gap-4 rounded-lg border p-4">
      <p className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">
        LEAVE TYPES · {year} · {typeCount} {typeCount === 1 ? 'type' : 'types'} · {totalDaysLeft} days left ·{' '}
        {totalDaysTaken} taken
      </p>
      <div className="flex flex-col gap-4">
        {limitedTypes.map((leaveType) => (
          <BalanceRow key={leaveType.id} leaveType={leaveType} />
        ))}
      </div>
    </aside>
  );
}
