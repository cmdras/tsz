import { Card, CardContent, CardHeader, CardTitle } from '#/components/ui/card';
import { getAvatarColor } from '#/lib/utils';
import type { MonthDayResponse } from '#/features/timesheets/timesheets.server';

interface MonthSidebarProps {
  days: MonthDayResponse[];
}

interface AggregatedRow {
  label: string;
  hours: number;
  color: string;
  isLeave: boolean;
}

function buildAggregatedRows(inMonthDays: MonthDayResponse[]): AggregatedRow[] {
  const customerHours = new Map<string, number>();
  const leaveHours = new Map<string, number>();

  for (const day of inMonthDays) {
    for (const entry of day.entries) {
      if (entry.kind === 'task' && entry.customerName) {
        customerHours.set(entry.customerName, (customerHours.get(entry.customerName) ?? 0) + entry.hours);
      } else if (entry.kind === 'leave' && entry.leaveTypeName) {
        leaveHours.set(entry.leaveTypeName, (leaveHours.get(entry.leaveTypeName) ?? 0) + entry.hours);
      }
    }
  }

  const rows: AggregatedRow[] = [
    ...Array.from(customerHours.entries()).map(([customerName, hours]) => ({
      label: customerName,
      hours,
      color: getAvatarColor(customerName),
      isLeave: false,
    })),
    ...Array.from(leaveHours.entries()).map(([leaveTypeName, hours]) => ({
      label: leaveTypeName,
      hours,
      color: '#f59e0b', // amber-500
      isLeave: true,
    })),
  ];

  return rows.toSorted((a, b) => b.hours - a.hours);
}

export function MonthSidebar({ days }: MonthSidebarProps) {
  const inMonthDays = days.filter((day) => day.isInMonth);

  const workdays = inMonthDays.filter((day) => day.entries.length > 0).length;
  const totalHours = inMonthDays.reduce((sum, day) => sum + day.totalHours, 0);

  const aggregatedRows = buildAggregatedRows(inMonthDays);

  return (
    <div className="flex w-56 shrink-0 flex-col gap-4">
      <Card>
        <CardHeader className="pb-2">
          <CardTitle className="text-sm font-medium">Totals</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground">
            {workdays} workdays · {totalHours}h
          </p>
        </CardContent>
      </Card>

      <Card>
        <CardHeader className="pb-2">
          <CardTitle className="text-sm font-medium">By customer</CardTitle>
        </CardHeader>
        <CardContent>
          {aggregatedRows.length === 0 ? (
            <p className="text-sm text-muted-foreground">No entries this month.</p>
          ) : (
            <ul className="flex flex-col gap-2">
              {aggregatedRows.map((row) => (
                <li
                  key={`${row.isLeave ? 'leave' : 'customer'}:${row.label}`}
                  className="flex items-center gap-2 text-sm"
                >
                  <span className="h-2.5 w-2.5 shrink-0 rounded-full" style={{ backgroundColor: row.color }} />
                  <span className="min-w-0 flex-1 truncate" title={row.label}>
                    {row.label}
                  </span>
                  <span className="shrink-0 text-muted-foreground">{row.hours}h</span>
                </li>
              ))}
            </ul>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
