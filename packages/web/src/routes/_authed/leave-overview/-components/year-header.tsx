import { useNavigate } from '@tanstack/react-router';
import { ChevronLeft, ChevronRight } from 'lucide-react';
import { Button } from '#/components/ui/button';
import type { LeaveOverviewTypeItem } from '#/features/leave-overview/leave-overview.server';

export const LEAVE_OVERVIEW_YEAR_MIN = 2000;
export const LEAVE_OVERVIEW_YEAR_MAX = 2100;

interface YearHeaderProps {
  year: number;
  types: LeaveOverviewTypeItem[];
}

function computeDaysLeft(types: LeaveOverviewTypeItem[]): number {
  return types
    .filter((leaveType) => leaveType.mode === 'Limited')
    .reduce((total, leaveType) => total + Math.max(0, leaveType.allowance - leaveType.takenDays), 0);
}

export function YearHeader({ year, types }: YearHeaderProps) {
  const navigate = useNavigate({ from: '/leave-overview/' });
  const currentYear = new Date().getFullYear();
  const isTodayYear = year === currentYear;
  const isAtMinYear = year <= LEAVE_OVERVIEW_YEAR_MIN;
  const isAtMaxYear = year >= LEAVE_OVERVIEW_YEAR_MAX;
  const daysLeft = computeDaysLeft(types);

  function handlePreviousYear() {
    void navigate({ search: { year: year - 1 } });
  }

  function handleNextYear() {
    void navigate({ search: { year: year + 1 } });
  }

  function handleToday() {
    void navigate({ search: { year: currentYear } });
  }

  return (
    <div className="flex items-start justify-between">
      <div>
        <h1 className="text-2xl font-semibold">Leave overview</h1>
        <p className="mt-0.5 text-sm text-muted-foreground">{daysLeft} days left</p>
      </div>
      <div className="flex items-center gap-2">
        <Button variant="outline" size="icon" disabled={isAtMinYear} onClick={handlePreviousYear}>
          <ChevronLeft className="h-4 w-4" />
          <span className="sr-only">Previous year</span>
        </Button>
        <span className="min-w-16 text-center font-medium">{year}</span>
        <Button variant="outline" size="icon" disabled={isAtMaxYear} onClick={handleNextYear}>
          <ChevronRight className="h-4 w-4" />
          <span className="sr-only">Next year</span>
        </Button>
        <Button variant="outline" disabled={isTodayYear} onClick={handleToday}>
          Today
        </Button>
      </div>
    </div>
  );
}
