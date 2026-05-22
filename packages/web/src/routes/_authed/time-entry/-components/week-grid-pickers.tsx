import { useState } from 'react';
import { PlusIcon } from 'lucide-react';
import { Button } from '#/components/ui/button';
import { Command, CommandEmpty, CommandGroup, CommandInput, CommandItem, CommandList } from '#/components/ui/command';
import { Popover, PopoverContent, PopoverTrigger } from '#/components/ui/popover';
import type { PickerLeaveTypeOption, PickerTaskOption } from '#/features/time-entries/time-entries.server';

export function TaskPickerPopover({
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
      <PopoverContent className="w-80 p-0" align="start" onCloseAutoFocus={(event) => event.preventDefault()}>
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

export function LeavePickerPopover({
  availableLeaveTypes,
  onPick,
}: {
  availableLeaveTypes: PickerLeaveTypeOption[];
  onPick: (leaveType: PickerLeaveTypeOption) => void;
}) {
  const [open, setOpen] = useState(false);

  function handleSelect(leaveType: PickerLeaveTypeOption) {
    onPick(leaveType);
    setOpen(false);
  }

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <Button variant="ghost" size="sm" className="text-muted-foreground">
          <PlusIcon className="mr-1 h-4 w-4" />
          Add leave
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-64 p-0" align="start" onCloseAutoFocus={(event) => event.preventDefault()}>
        <Command>
          <CommandInput placeholder="Search leave type…" />
          <CommandList>
            <CommandEmpty>No leave types found.</CommandEmpty>
            <CommandGroup>
              {availableLeaveTypes.map((leaveType) => (
                <CommandItem
                  key={leaveType.leaveTypeId}
                  value={leaveType.leaveTypeId}
                  keywords={[leaveType.name]}
                  onSelect={() => handleSelect(leaveType)}
                >
                  {leaveType.name}
                </CommandItem>
              ))}
            </CommandGroup>
          </CommandList>
        </Command>
      </PopoverContent>
    </Popover>
  );
}
