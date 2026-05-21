import { Link, useNavigate } from '@tanstack/react-router';
import { ChevronLeft, ChevronRight } from 'lucide-react';
import { useState } from 'react';
import { Button } from '#/components/ui/button';
import { Calendar } from '#/components/ui/calendar';
import { Popover, PopoverContent, PopoverTrigger } from '#/components/ui/popover';
import { addDays, fromIsoDateString, getIsoMonday, toIsoDateString } from '#/lib/date-utils';

interface WeekNavProps {
  weekStart: string;
}

export function WeekNav({ weekStart }: WeekNavProps) {
  const navigate = useNavigate({ from: '/_authed/time-entry/' });
  const [calendarOpen, setCalendarOpen] = useState(false);
  const currentDate = fromIsoDateString(weekStart);
  const todayMonday = toIsoDateString(getIsoMonday(new Date()));
  const prevWeek = toIsoDateString(addDays(currentDate, -7));
  const nextWeek = toIsoDateString(addDays(currentDate, 7));

  const weekLabel = currentDate.toLocaleDateString('en-GB', {
    day: 'numeric',
    month: 'short',
    year: 'numeric',
  });

  function handleCalendarSelect(date: Date | undefined) {
    setCalendarOpen(false);
    if (!date) return;
    void navigate({ search: { week: toIsoDateString(getIsoMonday(date)) } });
  }

  return (
    <div className="flex items-center gap-2">
      <Button variant="outline" size="icon" asChild>
        <Link search={{ week: prevWeek }}>
          <ChevronLeft className="h-4 w-4" />
          <span className="sr-only">Previous week</span>
        </Link>
      </Button>

      <Popover open={calendarOpen} onOpenChange={setCalendarOpen}>
        <PopoverTrigger asChild>
          <Button variant="outline" className="min-w-36">
            {weekLabel}
          </Button>
        </PopoverTrigger>
        <PopoverContent className="w-auto p-0" align="center">
          <Calendar
            mode="single"
            selected={currentDate}
            onSelect={handleCalendarSelect}
            weekStartsOn={1}
            initialFocus
          />
        </PopoverContent>
      </Popover>

      <Button variant="outline" size="icon" asChild>
        <Link search={{ week: nextWeek }}>
          <ChevronRight className="h-4 w-4" />
          <span className="sr-only">Next week</span>
        </Link>
      </Button>

      <Button variant="outline" asChild>
        <Link search={{ week: todayMonday }}>Today</Link>
      </Button>
    </div>
  );
}
