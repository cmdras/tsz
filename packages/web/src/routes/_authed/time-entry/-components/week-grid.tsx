import { useState } from 'react';
import { PlusIcon } from 'lucide-react';
import { Button } from '#/components/ui/button';
import { Command, CommandEmpty, CommandGroup, CommandInput, CommandItem, CommandList } from '#/components/ui/command';
import { Popover, PopoverContent, PopoverTrigger } from '#/components/ui/popover';
import { addDays, fromIsoDateString, DAYS_OF_WEEK } from '#/lib/date-utils';
import { getAvatarColor } from '#/lib/utils';
import type { PickerOptions, PickerTaskOption } from '#/features/time-entries/time-entries.server';

const WEEKEND_INDICES = new Set([5, 6]);

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
                  value={`${task.customerName} ${task.contractSubject} ${task.taskName}`}
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
    <div className={`border-l p-2 ${isWeekend ? 'bg-muted/40' : ''}`}>
      <div className="h-8 rounded border border-dashed border-muted-foreground/20 bg-muted/20" />
    </div>
  );
}

export function WeekGrid({ weekStart, pickerOptions }: WeekGridProps) {
  const monday = fromIsoDateString(weekStart);
  const [rows, setRows] = useState<GridRow[]>([]);

  const pickedIds = new Set(rows.map((row) => row.contractTaskId));
  const availableTasks = pickerOptions.availableTasks.filter((task) => !pickedIds.has(task.contractTaskId));

  const sortedRows = [...rows].toSorted((rowA, rowB) => {
    const customerComparison = rowA.customerName.localeCompare(rowB.customerName);
    if (customerComparison !== 0) return customerComparison;
    return rowA.taskName.localeCompare(rowB.taskName);
  });

  function handlePick(task: PickerTaskOption) {
    setRows((previousRows) => [
      ...previousRows,
      {
        contractTaskId: task.contractTaskId,
        customerName: task.customerName,
        contractSubject: task.contractSubject,
        taskName: task.taskName,
      },
    ]);
  }

  return (
    <div className="rounded-lg border">
      <div className="grid grid-cols-[auto_repeat(7,1fr)] border-b">
        <div className="p-3" />
        {DAYS_OF_WEEK.map((day, index) => {
          const date = addDays(monday, index);
          const isWeekend = WEEKEND_INDICES.has(index);
          return (
            <div
              key={day}
              className={`border-l p-3 text-center text-sm font-medium ${isWeekend ? 'bg-muted/40 text-muted-foreground' : ''}`}
            >
              <div>{day}</div>
              <div className="text-xs text-muted-foreground">
                {date.toLocaleDateString('en-GB', { day: 'numeric', month: 'short' })}
              </div>
            </div>
          );
        })}
      </div>

      {sortedRows.length === 0 && (
        <div className="p-8 text-center text-sm text-muted-foreground">
          No tasks yet — add a task or leave to get started
        </div>
      )}

      {sortedRows.map((row) => (
        <div key={row.contractTaskId} className="grid grid-cols-[auto_repeat(7,1fr)] border-b last:border-b-0">
          <TaskRowLabel row={row} />
          {DAYS_OF_WEEK.map((day, index) => (
            <DisabledCell key={day} isWeekend={WEEKEND_INDICES.has(index)} />
          ))}
        </div>
      ))}

      <div className="flex items-center gap-2 border-t p-2">
        <TaskPickerPopover availableTasks={availableTasks} onPick={handlePick} />
      </div>
    </div>
  );
}
