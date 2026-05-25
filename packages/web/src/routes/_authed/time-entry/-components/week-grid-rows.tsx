import { XIcon } from 'lucide-react';
import { addDays, DAYS_OF_WEEK, toIsoDateString } from '#/lib/date-utils';
import { cn, getAvatarColor } from '#/lib/utils';
import { HourCell, type HourCellHandle } from './hour-cell';
import type { GridRow, LeaveGridRow, TaskGridRow } from './week-grid-model';
import { getNextWeekdayIndex, getPrevWeekdayIndex } from './week-grid-navigation';

const WEEKEND_INDICES = new Set([5, 6]);

function formatTotal(total: number): string {
  return total > 0 ? `${total}h` : '—';
}

function TaskRowLabel({ row }: { row: TaskGridRow }) {
  const initials = row.customerName.slice(0, 2).toUpperCase();
  const avatarColor = getAvatarColor(row.customerName);

  return (
    <div className="flex items-center gap-2 p-3">
      <span
        className="flex h-7 w-7 shrink-0 items-center justify-center rounded-full text-xs font-semibold text-white"
        style={{ backgroundColor: avatarColor }}
      >
        {initials}
      </span>
      <div className="min-w-0">
        <div className="truncate font-semibold text-sm">{row.customerName}</div>
        <div className="truncate text-xs text-muted-foreground">
          {row.contractSubject} · {row.taskName}
        </div>
      </div>
    </div>
  );
}

function LeaveRowLabel({ row }: { row: LeaveGridRow }) {
  const initials = row.leaveTypeName.slice(0, 2).toUpperCase();

  return (
    <div className="flex items-center gap-2 p-3">
      <span className="flex h-7 w-7 shrink-0 items-center justify-center rounded-full bg-amber-500 text-xs font-semibold text-white">
        {initials}
      </span>
      <div className="min-w-0">
        <div className="truncate font-semibold text-sm">{row.leaveTypeName}</div>
      </div>
    </div>
  );
}

function DisabledCell({ isWeekend }: { isWeekend: boolean }) {
  return (
    <div className={cn('border-l p-2', isWeekend && 'bg-muted/40')}>
      <div className="relative flex h-8 items-center justify-center rounded border border-dashed border-muted-foreground/20 bg-muted/20">
        {isWeekend && <XIcon className="h-4 w-4 text-primary/40" />}
      </div>
    </div>
  );
}

interface WeekHeaderProps {
  monday: Date;
  todayIso: string;
  todayHasLeave: (dayIndex: number) => boolean;
}

export function WeekHeader({ monday, todayIso, todayHasLeave }: WeekHeaderProps) {
  return (
    <div className="grid grid-cols-[12rem_repeat(7,1fr)_4rem] border-b">
      <div className="p-3" />
      {DAYS_OF_WEEK.map((day, index) => {
        const date = addDays(monday, index);
        const dateIso = toIsoDateString(date);
        const isWeekend = WEEKEND_INDICES.has(index);
        const isToday = dateIso === todayIso;
        const todayIsLeave = isToday && todayHasLeave(index);

        return (
          <div
            key={day}
            className={cn(
              'border-l p-3 text-center text-sm font-medium',
              isWeekend && 'bg-muted/40 text-muted-foreground',
            )}
          >
            <div className="flex items-center justify-center gap-1">
              <span>{day}</span>
              {isToday && (
                <span
                  className={cn(
                    'rounded px-1 py-0.5 text-xs font-semibold',
                    todayIsLeave ? 'bg-amber-500 text-white' : 'bg-primary text-primary-foreground',
                  )}
                >
                  TODAY
                </span>
              )}
            </div>
            <div className="text-xs text-muted-foreground">
              {date.toLocaleDateString('en-GB', { day: 'numeric', month: 'short' })}
            </div>
          </div>
        );
      })}
      <div className="border-l p-3 text-center text-xs text-muted-foreground">Total</div>
    </div>
  );
}

interface WeekRowProps {
  row: GridRow;
  rowHours: (number | null)[];
  rowCellRefs: React.RefObject<HourCellHandle | null>[] | undefined;
  isSubmitted: boolean;
  getDailyOtherTotal: (dayIndex: number) => number;
  onHourCommit: (dayIndex: number, value: number | null) => void;
  weeklyTotal: number;
}

export function WeekRow({
  row,
  rowHours,
  rowCellRefs,
  isSubmitted,
  getDailyOtherTotal,
  onHourCommit,
  weeklyTotal,
}: WeekRowProps) {
  const isLeave = row.kind === 'leave';

  return (
    <div className="grid grid-cols-[12rem_repeat(7,1fr)_4rem] border-b last:border-b-0">
      {isLeave ? <LeaveRowLabel row={row as LeaveGridRow} /> : <TaskRowLabel row={row as TaskGridRow} />}
      {DAYS_OF_WEEK.map((day, index) => {
        const isWeekend = WEEKEND_INDICES.has(index);
        if (isWeekend) return <DisabledCell key={day} isWeekend />;
        if (isSubmitted) {
          const hourValue = rowHours[index];
          return (
            <div
              key={day}
              className={cn('border-l p-2 flex items-center justify-center text-sm', isLeave && 'text-amber-600')}
            >
              {hourValue ? `${hourValue}h` : '—'}
            </div>
          );
        }
        return (
          <HourCell
            key={day}
            ref={rowCellRefs?.[index]}
            value={rowHours[index] ?? null}
            dailyOtherTotal={getDailyOtherTotal(index)}
            isWeekend={false}
            isLeave={isLeave}
            onCommit={(value) => onHourCommit(index, value)}
            onFocusNext={() => {
              const nextIndex = getNextWeekdayIndex(index);
              if (nextIndex !== null) rowCellRefs?.[nextIndex]?.current?.triggerFocus();
            }}
            onFocusPrev={() => {
              const prevIndex = getPrevWeekdayIndex(index);
              if (prevIndex !== null) rowCellRefs?.[prevIndex]?.current?.triggerFocus();
            }}
          />
        );
      })}
      <div className="border-l p-2 flex items-center justify-center text-sm font-medium text-muted-foreground">
        {formatTotal(weeklyTotal)}
      </div>
    </div>
  );
}

interface WeekDailyTotalsProps {
  sortedRowIds: string[];
  getDailyTotal: (dayIndex: number) => number;
}

export function WeekDailyTotals({ sortedRowIds, getDailyTotal }: WeekDailyTotalsProps) {
  if (sortedRowIds.length === 0) return null;
  const weeklyGrandTotal = DAYS_OF_WEEK.reduce((sum, _, index) => sum + getDailyTotal(index), 0);

  return (
    <div className="grid grid-cols-[12rem_repeat(7,1fr)_4rem] border-t">
      <div className="p-2 text-xs text-muted-foreground flex items-center pl-3">Daily total</div>
      {DAYS_OF_WEEK.map((day, index) => {
        const isWeekend = WEEKEND_INDICES.has(index);
        const total = getDailyTotal(index);
        return (
          <div
            key={day}
            className={cn(
              'border-l p-2 text-center text-sm font-medium',
              isWeekend && 'bg-muted/40 text-muted-foreground',
              !isWeekend && total === 0 && 'text-muted-foreground/40',
            )}
          >
            {isWeekend ? '—' : formatTotal(total)}
          </div>
        );
      })}
      <div className="border-l p-2 text-center text-sm font-medium text-muted-foreground">
        {formatTotal(weeklyGrandTotal)}
      </div>
    </div>
  );
}
