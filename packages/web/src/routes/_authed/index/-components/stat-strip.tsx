import type { HomeStats } from '#/features/home/home-view';

interface StatStripProps {
  stats: HomeStats;
}

interface StatColumnProps {
  value: string;
  label: string;
}

function StatColumn({ value, label }: StatColumnProps) {
  return (
    <div className="flex flex-1 flex-col items-center gap-1 py-4">
      <span className="text-2xl font-bold text-primary">{value}</span>
      <span className="text-xs text-muted-foreground">{label}</span>
    </div>
  );
}

export function StatStrip({ stats }: StatStripProps) {
  return (
    <div className="border-t border-border">
      <div className="flex divide-x divide-border">
        <StatColumn value={`${stats.weeksSubmitted}/${stats.weeksTotal}`} label="weeks submitted" />
        <StatColumn value={`${stats.loggedThisMonth}h`} label="logged this month" />
        <StatColumn value={String(stats.leaveDaysLeft)} label="leave days left" />
      </div>
    </div>
  );
}
