import { Card, CardContent } from '#/components/ui/card';
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
    <aside className="w-64 shrink-0">
      <Card className="border-primary">
        <CardContent className="p-4">
          <p className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">Leave Types · {year}</p>
          <div className="mt-3 flex items-baseline gap-1.5">
            <span className="text-4xl font-bold text-primary">{totalDaysLeft}</span>
            <span className="text-sm text-muted-foreground">days left</span>
          </div>
          <p className="mt-1.5 text-xs text-muted-foreground">{totalDaysTaken} taken</p>
          {limitedTypes.length > 0 && (
            <>
              <div className="mt-4 flex items-center justify-between">
                <p className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">By type</p>
                <p className="text-xs text-muted-foreground">
                  {typeCount} {typeCount === 1 ? 'type' : 'types'}
                </p>
              </div>
              <ul className="mt-2 flex flex-col gap-3">
                {limitedTypes.map((leaveType) => (
                  <BalanceRow key={leaveType.id} leaveType={leaveType} />
                ))}
              </ul>
            </>
          )}
        </CardContent>
      </Card>
    </aside>
  );
}
