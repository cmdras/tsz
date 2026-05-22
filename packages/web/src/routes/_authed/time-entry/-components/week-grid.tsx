import { useState } from 'react';
import { PlusIcon, XIcon } from 'lucide-react';
import { Button } from '#/components/ui/button';
import { Command, CommandEmpty, CommandGroup, CommandInput, CommandItem, CommandList } from '#/components/ui/command';
import { Popover, PopoverContent, PopoverTrigger } from '#/components/ui/popover';
import { addDays, fromIsoDateString, toIsoDateString, DAYS_OF_WEEK } from '#/lib/date-utils';
import { cn, getAvatarColor } from '#/lib/utils';
import type { PickerOptions, PickerTaskOption } from '#/features/time-entries/time-entries.server';
import { HourCell } from './hour-cell';

const WEEKEND_INDICES = new Set([5, 6]);
const DAYS_COUNT = 7;

interface GridRow {
  contractTaskId: string;
  customerName: string;
  contractSubject: string;
  taskName: string;
}

interface WeekGridProps {
  weekStart: string;
  pickerOptions: PickerOptions;
}

function TaskPickerPopover({
  availableTasks,
  onPick,
}: {
  availableTasks: PickerTaskOption[];
  onPick: (task: PickerTaskOption) => void;
}) {
  const [open, setOpen] = useState(false);

  function handleSelect(task: PickerTaskOption) {
    onPick(task);
    setOpen(false);
  }

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <Button variant="ghost" size="sm" className="text-muted-foreground">
          <PlusIcon className="mr-1 h-4 w-4" />
          Add task
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-80 p-0" align="start">
        <Command>
          <CommandInput placeholder="Search customer, contract or task…" />
          <CommandList>
            <CommandEmpty>No tasks found.</CommandEmpty>
            <CommandGroup>
              {availableTasks.map((task) => (
                <CommandItem
                  key={task.contractTaskId}
                  value={task.contractTaskId}
                  keywords={[task.customerName, task.contractSubject, task.taskName]}
                  onSelect={() => handleSelect(task)}
                >
                  <span className="font-medium">{task.customerName}</span>
                  <span className="mx-1 text-muted-foreground">·</span>
                  <span className="text-muted-foreground">
                    {task.contractSubject} · {task.taskName}
                  </span>
                </CommandItem>
              ))}
            </CommandGroup>
          </CommandList>
        </Command>
      </PopoverContent>
    </Popover>
  );
}

function TaskRowLabel({ row }: { row: GridRow }) {
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

function DisabledCell({ isWeekend }: { isWeekend: boolean }) {
  return (
    <div className={cn('border-l p-2', isWeekend && 'bg-muted/40')}>
      <div className="relative flex h-8 items-center justify-center rounded border border-dashed border-muted-foreground/20 bg-muted/20">
        {isWeekend && <XIcon className="h-4 w-4 text-primary/40" />}
      </div>
    </div>
  );
}

function formatTotal(total: number): string {
  return total > 0 ? `${total}h` : '—';
}

export function WeekGrid({ weekStart, pickerOptions }: WeekGridProps) {
  const monday = fromIsoDateString(weekStart);
  const todayIso = toIsoDateString(new Date());

  const [rows, setRows] = useState<GridRow[]>([]);
  const [hours, setHours] = useState<Record<string, (number | null)[]>>({});

  const pickedIds = new Set(rows.map((row) => row.contractTaskId));
  const availableTasks = pickerOptions.availableTasks.filter((task) => !pickedIds.has(task.contractTaskId));

  const sortedRows = [...rows].toSorted((rowA, rowB) => {
    const customerComparison = rowA.customerName.localeCompare(rowB.customerName);
    if (customerComparison !== 0) return customerComparison;
    return rowA.taskName.localeCompare(rowB.taskName);
  });

  function getRowHours(contractTaskId: string): (number | null)[] {
    return hours[contractTaskId] ?? Array(DAYS_COUNT).fill(null);
  }

  function getDailyTotal(dayIndex: number): number {
    return sortedRows.reduce((sum, row) => sum + (getRowHours(row.contractTaskId)[dayIndex] ?? 0), 0);
  }

  function getDailyOtherTotal(contractTaskId: string, dayIndex: number): number {
    return sortedRows
      .filter((row) => row.contractTaskId !== contractTaskId)
      .reduce((sum, row) => sum + (getRowHours(row.contractTaskId)[dayIndex] ?? 0), 0);
  }

  function getWeeklyRowTotal(contractTaskId: string): number {
    return getRowHours(contractTaskId).reduce((sum, hour) => sum + (hour ?? 0), 0);
  }

  function handlePick(task: PickerTaskOption) {
    setRows((previousRows) => [...previousRows, task]);
    setHours((previousHours) => ({
      ...previousHours,
      [task.contractTaskId]: Array(DAYS_COUNT).fill(null),
    }));
  }

  function handleHourCommit(contractTaskId: string, dayIndex: number, value: number | null) {
    setHours((previousHours) => {
      const rowHours = previousHours[contractTaskId] ?? Array(DAYS_COUNT).fill(null);
      const updated = rowHours.map((existingHour, index) => (index === dayIndex ? value : existingHour));
      return { ...previousHours, [contractTaskId]: updated };
    });
  }

  return (
    <div className="rounded-lg border">
      {/* Header row */}
      <div className="grid grid-cols-[12rem_repeat(7,1fr)_4rem] border-b">
        <div className="p-3" />
        {DAYS_OF_WEEK.map((day, index) => {
          const date = addDays(monday, index);
          const dateIso = toIsoDateString(date);
          const isWeekend = WEEKEND_INDICES.has(index);
          const isToday = dateIso === todayIso;
          const dailyTotal = getDailyTotal(index);

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
                      dailyTotal > 0 ? 'bg-primary text-primary-foreground' : 'bg-muted text-muted-foreground',
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

      {/* Empty state */}
      {sortedRows.length === 0 && (
        <div className="p-8 text-center text-sm text-muted-foreground">
          No tasks yet — add a task or leave to get started
        </div>
      )}

      {/* Task rows */}
      {sortedRows.map((row) => {
        const weeklyTotal = getWeeklyRowTotal(row.contractTaskId);
        const rowHours = getRowHours(row.contractTaskId);

        return (
          <div key={row.contractTaskId} className="grid grid-cols-[12rem_repeat(7,1fr)_4rem] border-b last:border-b-0">
            <TaskRowLabel row={row} />
            {DAYS_OF_WEEK.map((day, index) => {
              const isWeekend = WEEKEND_INDICES.has(index);
              return isWeekend ? (
                <DisabledCell key={day} isWeekend />
              ) : (
                <HourCell
                  key={day}
                  value={rowHours[index] ?? null}
                  dailyOtherTotal={getDailyOtherTotal(row.contractTaskId, index)}
                  isWeekend={false}
                  onCommit={(value) => handleHourCommit(row.contractTaskId, index, value)}
                />
              );
            })}
            <div className="border-l p-2 flex items-center justify-center text-sm font-medium text-muted-foreground">
              {formatTotal(weeklyTotal)}
            </div>
          </div>
        );
      })}

      {/* Daily totals footer */}
      {sortedRows.length > 0 && (
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
            {formatTotal(DAYS_OF_WEEK.reduce((sum, _, index) => sum + getDailyTotal(index), 0))}
          </div>
        </div>
      )}

      {/* Add task button */}
      <div className="flex items-center gap-2 border-t p-2">
        <TaskPickerPopover availableTasks={availableTasks} onPick={handlePick} />
      </div>
    </div>
  );
}
