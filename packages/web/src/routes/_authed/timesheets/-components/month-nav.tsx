import { useNavigate } from '@tanstack/react-router';
import { ChevronLeft, ChevronRight } from 'lucide-react';
import { useState } from 'react';
import { toast } from 'sonner';
import { Button } from '#/components/ui/button';
import { Calendar } from '#/components/ui/calendar';
import { Popover, PopoverContent, PopoverTrigger } from '#/components/ui/popover';

interface MonthNavProps {
  yearMonth: string;
}

function toYearMonth(date: Date): string {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  return `${year}-${month}`;
}

function addMonths(yearMonth: string, delta: number): string {
  const year = parseInt(yearMonth.slice(0, 4), 10);
  const month = parseInt(yearMonth.slice(5, 7), 10);
  const date = new Date(year, month - 1 + delta, 1);
  return toYearMonth(date);
}

function toFirstOfMonth(yearMonth: string): Date {
  const year = parseInt(yearMonth.slice(0, 4), 10);
  const month = parseInt(yearMonth.slice(5, 7), 10);
  return new Date(year, month - 1, 1);
}

export function MonthNav({ yearMonth }: MonthNavProps) {
  const navigate = useNavigate({ from: '/timesheets/' });
  const [calendarOpen, setCalendarOpen] = useState(false);

  const currentDate = toFirstOfMonth(yearMonth);
  const todayYearMonth = toYearMonth(new Date());
  const prevMonth = addMonths(yearMonth, -1);
  const nextMonth = addMonths(yearMonth, 1);

  const monthLabel = currentDate.toLocaleDateString('en-GB', {
    month: 'long',
    year: 'numeric',
  });

  function handleCalendarSelect(date: Date | undefined) {
    setCalendarOpen(false);
    if (!date) return;
    void navigate({ search: { month: toYearMonth(date) } });
  }

  function handleExportMonth() {
    toast.info('Export coming soon');
  }

  return (
    <div className="flex items-center gap-2">
      <Button variant="outline" size="icon" onClick={() => void navigate({ search: { month: prevMonth } })}>
        <ChevronLeft className="h-4 w-4" />
        <span className="sr-only">Previous month</span>
      </Button>

      <Popover open={calendarOpen} onOpenChange={setCalendarOpen}>
        <PopoverTrigger asChild>
          <Button variant="outline" className="min-w-44">
            {monthLabel}
          </Button>
        </PopoverTrigger>
        <PopoverContent className="w-auto p-0" align="center">
          <Calendar mode="single" selected={currentDate} onSelect={handleCalendarSelect} weekStartsOn={1} autoFocus />
        </PopoverContent>
      </Popover>

      <Button variant="outline" size="icon" onClick={() => void navigate({ search: { month: nextMonth } })}>
        <ChevronRight className="h-4 w-4" />
        <span className="sr-only">Next month</span>
      </Button>

      <Button variant="outline" onClick={() => void navigate({ search: { month: todayYearMonth } })}>
        Today
      </Button>

      <Button variant="outline" onClick={handleExportMonth}>
        Export month
      </Button>
    </div>
  );
}
