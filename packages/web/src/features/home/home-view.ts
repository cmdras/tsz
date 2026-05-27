import { addDays, getIsoMonday, getIsoWeekNumber, toIsoDateString } from '#/lib/date-utils';
import type { components } from '#/api/schema';

type MonthResponse = components['schemas']['MonthResponse'];
type LeaveOverviewResponse = components['schemas']['LeaveOverviewResponse'];

export type HomeTone = 'caughtUp' | 'tasks';

export interface HomeWeekTask {
  weekStart: string;
  weekNumber: number;
  dateRange: string;
  loggedHours: number;
  status: 'empty' | 'draft';
  isPrimary: boolean;
}

export interface HomeStats {
  weeksSubmitted: number;
  weeksTotal: number;
  loggedThisMonth: number;
  leaveDaysLeft: number;
}

export interface HomeViewModel {
  greetingName: string;
  tone: HomeTone;
  tasks: HomeWeekTask[];
  stats: HomeStats;
}

const dutchDateFormatter = new Intl.DateTimeFormat('nl-NL', { day: 'numeric', month: 'long' });
const dutchDayFormatter = new Intl.DateTimeFormat('nl-NL', { day: 'numeric' });

function formatDutchDateRange(monday: Date, sunday: Date): string {
  const mondayMonth = monday.getMonth();
  const sundayMonth = sunday.getMonth();

  if (mondayMonth === sundayMonth) {
    const dayStart = dutchDayFormatter.format(monday);
    const dayEnd = dutchDateFormatter.format(sunday);
    return `${dayStart} – ${dayEnd}`;
  }

  const start = dutchDateFormatter.format(monday);
  const end = dutchDateFormatter.format(sunday);
  return `${start} – ${end}`;
}

/**
 * Enumerates every ISO week Monday that overlaps the given month.
 * A week overlaps if any of its 7 days falls within the month boundaries.
 */
function enumerateWeekMondaysForMonth(yearMonth: string): Date[] {
  const [yearStr, monthStr] = yearMonth.split('-');
  const year = Number(yearStr);
  const month = Number(monthStr) - 1; // 0-indexed

  const firstOfMonth = new Date(year, month, 1);
  const lastOfMonth = new Date(year, month + 1, 0);

  const mondays: Date[] = [];
  let current = getIsoMonday(firstOfMonth);

  while (current <= lastOfMonth) {
    mondays.push(new Date(current));
    current = addDays(current, 7);
  }

  return mondays;
}

function computeLeaveDaysLeft(leaveOverview: LeaveOverviewResponse): number {
  return leaveOverview.types
    .filter((leaveType) => leaveType.mode === 'Limited')
    .reduce((total, leaveType) => total + Math.max(0, leaveType.allowance - leaveType.takenDays), 0);
}

export function buildHomeViewModel(
  month: MonthResponse,
  leaveOverview: LeaveOverviewResponse,
  today: Date,
  greetingName: string,
): HomeViewModel {
  const submittedWeekStarts = new Set(month.weekSubmissions.map((submission) => submission.weekStart));

  const weekMondays = enumerateWeekMondaysForMonth(month.yearMonth);
  const weeksTotal = weekMondays.length;

  const todayMonday = toIsoDateString(getIsoMonday(today));

  // Build a map from date string to totalHours for quick lookup
  const dayHoursMap = new Map<string, number>();
  for (const day of month.days) {
    if (day.isInMonth) {
      dayHoursMap.set(day.date, day.totalHours);
    }
  }

  const loggedThisMonth = month.days.filter((day) => day.isInMonth).reduce((total, day) => total + day.totalHours, 0);

  const weeksSubmitted = submittedWeekStarts.size;

  const tasks: HomeWeekTask[] = [];

  for (const monday of weekMondays) {
    const weekStart = toIsoDateString(monday);

    if (submittedWeekStarts.has(weekStart)) {
      continue;
    }

    // Sum hours for days in this week that fall within the fetched month
    let loggedHours = 0;
    for (let dayOffset = 0; dayOffset < 7; dayOffset++) {
      const day = addDays(monday, dayOffset);
      const dayString = toIsoDateString(day);
      const hours = dayHoursMap.get(dayString);
      if (hours !== undefined) {
        loggedHours += hours;
      }
    }

    const sunday = addDays(monday, 6);
    const dateRange = formatDutchDateRange(monday, sunday);
    const weekNumber = getIsoWeekNumber(monday);

    tasks.push({
      weekStart,
      weekNumber,
      dateRange,
      loggedHours,
      status: loggedHours === 0 ? 'empty' : 'draft',
      isPrimary: weekStart === todayMonday,
    });
  }

  // Tasks already ordered chronologically (oldest first) since weekMondays is ascending

  const leaveDaysLeft = computeLeaveDaysLeft(leaveOverview);
  const tone: HomeTone = tasks.length > 0 ? 'tasks' : 'caughtUp';

  return {
    greetingName,
    tone,
    tasks,
    stats: {
      weeksSubmitted,
      weeksTotal,
      loggedThisMonth,
      leaveDaysLeft,
    },
  };
}
