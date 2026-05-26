import { useNavigate } from '@tanstack/react-router';
import { Card, CardContent } from '#/components/ui/card';
import { getAvatarColor } from '#/lib/utils';
import type { MonthDayResponse, WeekSubmissionStatusResponse } from '#/features/timesheets/timesheets.server';
import { fromIsoDateString, toIsoDateString, getIsoMonday } from '#/lib/date-utils';

interface MonthSidebarProps {
  days: MonthDayResponse[];
  weekSubmissions: WeekSubmissionStatusResponse[];
  today: string;
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
      color: '#f59e0b',
      isLeave: true,
    })),
  ];

  return rows.toSorted((a, b) => b.hours - a.hours);
}

function countUnsubmittedPastWeeks(
  days: MonthDayResponse[],
  weekSubmissions: WeekSubmissionStatusResponse[],
  today: string,
): number {
  const submittedWeekStarts = new Set(weekSubmissions.map((submission) => submission.weekStart));

  const weekRows: MonthDayResponse[][] = [];
  for (let index = 0; index < days.length; index += 7) {
    weekRows.push(days.slice(index, index + 7));
  }

  return weekRows.filter((rowDays) => {
    const sundayDate = rowDays[6].date;
    if (sundayDate >= today) return false;
    const mondayDate = rowDays[0].date;
    if (submittedWeekStarts.has(mondayDate)) return false;
    return rowDays.some((day) => day.isInMonth && day.entries.length > 0);
  }).length;
}

function isoMondayForDate(dateString: string): string {
  const date = fromIsoDateString(dateString);
  return toIsoDateString(getIsoMonday(date));
}

export function MonthSidebar({ days, weekSubmissions, today }: MonthSidebarProps) {
  const navigate = useNavigate();
  const inMonthDays = days.filter((day) => day.isInMonth);

  const daysWithEntries = inMonthDays.filter((day) => day.entries.length > 0).length;
  const totalHours = inMonthDays.reduce((sum, day) => sum + day.totalHours, 0);
  const holidayHours = inMonthDays.reduce((sum, day) => {
    return sum + day.entries.filter((entry) => entry.kind === 'leave').reduce((s, entry) => s + entry.hours, 0);
  }, 0);

  const aggregatedRows = buildAggregatedRows(inMonthDays);
  const unsubmittedPastWeeksCount = countUnsubmittedPastWeeks(days, weekSubmissions, today);

  function handleReviewSubmission() {
    const firstUnsubmitted = days.find((day) => {
      if (!day.isInMonth || day.entries.length === 0) return false;
      const submittedWeekStarts = new Set(weekSubmissions.map((s) => s.weekStart));
      const mondayDate = isoMondayForDate(day.date);
      const sundayDate = days.find((d) => d.date > mondayDate && new Date(d.date + 'T00:00:00').getDay() === 0)?.date;
      return sundayDate && sundayDate < today && !submittedWeekStarts.has(mondayDate);
    });
    const weekParam = firstUnsubmitted ? isoMondayForDate(firstUnsubmitted.date) : isoMondayForDate(today);
    void navigate({ to: '/time-entry', search: { week: weekParam } });
  }

  return (
    <div className="flex w-64 shrink-0 flex-col gap-4">
      <Card className="border-primary">
        <CardContent className="p-4">
          <p className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">
            Totals / Approved entries
          </p>

          <div className="mt-3 flex items-baseline gap-1.5">
            <span className="text-4xl font-bold text-primary">{daysWithEntries}</span>
            <span className="text-sm text-muted-foreground">workdays</span>
          </div>

          <p className="mt-1.5 text-xs text-muted-foreground">
            {totalHours}h logged{holidayHours > 0 ? ` · ${holidayHours}h holidays` : ''}
          </p>

          {aggregatedRows.length > 0 && (
            <>
              <div className="mt-4 flex items-center justify-between">
                <p className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">By customer</p>
                <p className="text-xs text-muted-foreground">{aggregatedRows.length} customers</p>
              </div>
              <ul className="mt-2 flex flex-col gap-3">
                {aggregatedRows.map((row) => (
                  <li key={`${row.isLeave ? 'leave' : 'customer'}:${row.label}`}>
                    <div className="flex items-center justify-between text-sm">
                      <div className="flex min-w-0 items-center gap-1.5">
                        <span className="h-2 w-2 shrink-0 rounded-full" style={{ backgroundColor: row.color }} />
                        <span className="min-w-0 truncate" title={row.label}>
                          {row.label}
                        </span>
                      </div>
                      <span className="ml-2 shrink-0 text-muted-foreground">{row.hours}h</span>
                    </div>
                    <div className="mt-1 h-0.5 overflow-hidden rounded-full bg-muted">
                      <div
                        className="h-full rounded-full bg-primary"
                        style={{ width: totalHours > 0 ? `${(row.hours / totalHours) * 100}%` : '0%' }}
                      />
                    </div>
                  </li>
                ))}
              </ul>
            </>
          )}

          {aggregatedRows.length === 0 && <p className="mt-4 text-sm text-muted-foreground">No entries this month.</p>}
        </CardContent>
      </Card>

      {unsubmittedPastWeeksCount > 0 && (
        <Card className="border-amber-500 bg-amber-500/10">
          <CardContent className="p-4">
            <p className="text-sm font-medium text-amber-400">
              {unsubmittedPastWeeksCount} week{unsubmittedPastWeeksCount > 1 ? 's' : ''} awaiting submission
            </p>
            <button
              onClick={handleReviewSubmission}
              className="mt-1 text-xs text-amber-400 underline hover:text-amber-300"
            >
              Review submission
            </button>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
