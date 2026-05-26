import type { MonthDayResponse, WeekSubmissionStatusResponse } from '#/features/timesheets/timesheets.server';
import { cn } from '#/lib/utils';
import { DayCell } from './day-cell';

interface MonthGridProps {
  days: MonthDayResponse[];
  today: string;
  weekSubmissions: WeekSubmissionStatusResponse[];
}

const DAY_HEADERS = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'] as const;

function isWeekendDate(dateString: string): boolean {
  const date = new Date(dateString + 'T00:00:00');
  const dayOfWeek = date.getDay();
  return dayOfWeek === 0 || dayOfWeek === 6;
}

function isoMondayOfRow(rowDays: MonthDayResponse[]): string {
  return rowDays[0].date;
}

function isoSundayOfRow(rowDays: MonthDayResponse[]): string {
  return rowDays[6].date;
}

function isPastUnsubmitted(rowDays: MonthDayResponse[], today: string, submittedWeekStarts: Set<string>): boolean {
  const sundayDate = isoSundayOfRow(rowDays);
  const isPast = sundayDate < today;
  if (!isPast) return false;
  const mondayDate = isoMondayOfRow(rowDays);
  return !submittedWeekStarts.has(mondayDate);
}

export function MonthGrid({ days, today, weekSubmissions }: MonthGridProps) {
  const submittedWeekStarts = new Set(weekSubmissions.map((submission) => submission.weekStart));

  // Split flat day list into chunks of 7 (one per week-row)
  const weekRows: MonthDayResponse[][] = [];
  for (let index = 0; index < days.length; index += 7) {
    weekRows.push(days.slice(index, index + 7));
  }

  return (
    <div className="overflow-hidden rounded-lg border">
      <div className="grid grid-cols-7">
        {DAY_HEADERS.map((header) => (
          <div
            key={header}
            className={`border-b px-2 py-1 text-center text-sm font-medium text-muted-foreground${header === 'Sat' || header === 'Sun' ? ' bg-muted/50' : ''}`}
          >
            {header}
          </div>
        ))}
      </div>

      {weekRows.map((rowDays) => (
        <div
          key={isoMondayOfRow(rowDays)}
          className={cn(
            'grid grid-cols-7',
            isPastUnsubmitted(rowDays, today, submittedWeekStarts) && 'border-l-4 border-l-amber-500',
          )}
        >
          {rowDays.map((day) => (
            <DayCell key={day.date} day={day} isToday={day.date === today} isWeekend={isWeekendDate(day.date)} />
          ))}
        </div>
      ))}
    </div>
  );
}
