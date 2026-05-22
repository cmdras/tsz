import { createRef, forwardRef, useEffect, useImperativeHandle, useRef, useState } from 'react';
import { addDays, fromIsoDateString, toIsoDateString, DAYS_OF_WEEK } from '#/lib/date-utils';
import { cn } from '#/lib/utils';
import type { PickerLeaveTypeOption, PickerTaskOption } from '#/features/time-entries/time-entries.server';
import { HourCell, type HourCellHandle } from './hour-cell';
import { type GridRow, type LeaveGridRow, type TaskGridRow, hoursFromSaved, rowsFromSaved } from './week-grid-model';
import type { WeekGridProps } from './week-grid-model';
import { LeavePickerPopover, TaskPickerPopover } from './week-grid-pickers';
import { DisabledCell, LeaveRowLabel, TaskRowLabel } from './week-grid-rows';

export type { WeekGridHandle } from './week-grid-model';

const WEEKEND_INDICES = new Set([5, 6]);
const DAYS_COUNT = 7;

function formatTotal(total: number): string {
  return total > 0 ? `${total}h` : '—';
}

function getNextWeekdayIndex(dayIndex: number): number | null {
  return dayIndex < 4 ? dayIndex + 1 : null;
}

function getPrevWeekdayIndex(dayIndex: number): number | null {
  return dayIndex > 0 ? dayIndex - 1 : null;
}

export const WeekGrid = forwardRef<import('./week-grid-model').WeekGridHandle, WeekGridProps>(function WeekGrid(
  { weekStart, savedRows, pickerOptions, isSubmitted, onDirtyChange },
  ref,
) {
  const monday = fromIsoDateString(weekStart);
  const todayIso = toIsoDateString(new Date());

  const [rows, setRows] = useState<GridRow[]>(() => rowsFromSaved(savedRows));
  const [hours, setHours] = useState<Record<string, (number | null)[]>>(() => hoursFromSaved(savedRows));
  const [pendingFocusId, setPendingFocusId] = useState<string | null>(null);
  const cellRefsMap = useRef(
    new Map<string, React.RefObject<HourCellHandle | null>[]>(
      savedRows
        .filter((row) => row.contractTaskId || row.leaveTypeId)
        .map((row) => [
          row.contractTaskId ?? row.leaveTypeId!,
          Array.from({ length: DAYS_COUNT }, () => createRef<HourCellHandle | null>()),
        ]),
    ),
  );
  const isDirtyRef = useRef(false);

  useEffect(() => {
    if (pendingFocusId === null) return;
    cellRefsMap.current.get(pendingFocusId)?.[0]?.current?.triggerFocus();
    setPendingFocusId(null);
  }, [pendingFocusId]);

  useImperativeHandle(ref, () => ({
    resetDirty() {
      isDirtyRef.current = false;
    },
    getCells() {
      return rows.flatMap((row) => {
        const rowHours = hours[row.rowId] ?? Array(DAYS_COUNT).fill(null);
        return rowHours.flatMap((hourValue, dayIndex) => {
          if (hourValue === null || hourValue <= 0) return [];
          const date = addDays(monday, dayIndex);
          if (row.kind === 'task') {
            return [
              { contractTaskId: row.contractTaskId, leaveTypeId: null, date: toIsoDateString(date), hours: hourValue },
            ];
          }
          return [
            { contractTaskId: null, leaveTypeId: row.leaveTypeId, date: toIsoDateString(date), hours: hourValue },
          ];
        });
      });
    },
  }));

  const pickedTaskIds = new Set(
    rows.filter((row): row is TaskGridRow => row.kind === 'task').map((row) => row.contractTaskId),
  );
  const pickedLeaveTypeIds = new Set(
    rows.filter((row): row is LeaveGridRow => row.kind === 'leave').map((row) => row.leaveTypeId),
  );
  const availableTasks = pickerOptions.availableTasks.filter((task) => !pickedTaskIds.has(task.contractTaskId));
  const availableLeaveTypes = pickerOptions.availableLeaveTypes.filter(
    (leaveType) => !pickedLeaveTypeIds.has(leaveType.leaveTypeId),
  );

  const sortedRows = [...rows].toSorted((rowA, rowB) => {
    if (rowA.kind !== rowB.kind) return rowA.kind === 'task' ? -1 : 1;
    if (rowA.kind === 'task' && rowB.kind === 'task') {
      const customerComparison = rowA.customerName.localeCompare(rowB.customerName);
      if (customerComparison !== 0) return customerComparison;
      return rowA.taskName.localeCompare(rowB.taskName);
    }
    if (rowA.kind === 'leave' && rowB.kind === 'leave') {
      return rowA.leaveTypeName.localeCompare(rowB.leaveTypeName);
    }
    return 0;
  });

  function getRowHours(rowId: string): (number | null)[] {
    return hours[rowId] ?? Array(DAYS_COUNT).fill(null);
  }

  function getDailyTotal(dayIndex: number): number {
    return sortedRows.reduce((sum, row) => sum + (getRowHours(row.rowId)[dayIndex] ?? 0), 0);
  }

  function getDailyOtherTotal(rowId: string, dayIndex: number): number {
    return sortedRows
      .filter((row) => row.rowId !== rowId)
      .reduce((sum, row) => sum + (getRowHours(row.rowId)[dayIndex] ?? 0), 0);
  }

  function getWeeklyRowTotal(rowId: string): number {
    return getRowHours(rowId).reduce((sum, hour) => sum + (hour ?? 0), 0);
  }

  function markDirty() {
    if (!isDirtyRef.current) {
      isDirtyRef.current = true;
      onDirtyChange(true);
    }
  }

  function handlePickTask(task: PickerTaskOption) {
    const newRow: TaskGridRow = {
      kind: 'task',
      rowId: task.contractTaskId,
      contractTaskId: task.contractTaskId,
      customerName: task.customerName,
      contractSubject: task.contractSubject,
      taskName: task.taskName,
    };
    setRows((previousRows) => [...previousRows, newRow]);
    setHours((previousHours) => ({ ...previousHours, [task.contractTaskId]: Array(DAYS_COUNT).fill(null) }));
    cellRefsMap.current.set(
      task.contractTaskId,
      Array.from({ length: DAYS_COUNT }, () => createRef<HourCellHandle | null>()),
    );
    setPendingFocusId(task.contractTaskId);
    markDirty();
  }

  function handlePickLeave(leaveType: PickerLeaveTypeOption) {
    const newRow: LeaveGridRow = {
      kind: 'leave',
      rowId: leaveType.leaveTypeId,
      leaveTypeId: leaveType.leaveTypeId,
      leaveTypeName: leaveType.name,
    };
    setRows((previousRows) => [...previousRows, newRow]);
    setHours((previousHours) => ({ ...previousHours, [leaveType.leaveTypeId]: Array(DAYS_COUNT).fill(null) }));
    cellRefsMap.current.set(
      leaveType.leaveTypeId,
      Array.from({ length: DAYS_COUNT }, () => createRef<HourCellHandle | null>()),
    );
    setPendingFocusId(leaveType.leaveTypeId);
    markDirty();
  }

  function handleHourCommit(rowId: string, dayIndex: number, value: number | null) {
    setHours((previousHours) => {
      const rowHours = previousHours[rowId] ?? Array(DAYS_COUNT).fill(null);
      const updated = rowHours.map((existingHour, index) => (index === dayIndex ? value : existingHour));
      return { ...previousHours, [rowId]: updated };
    });
    markDirty();
  }

  function todayHasLeave(dayIndex: number): boolean {
    return sortedRows
      .filter((row): row is LeaveGridRow => row.kind === 'leave')
      .some((row) => (getRowHours(row.rowId)[dayIndex] ?? 0) > 0);
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

      {/* Empty state */}
      {sortedRows.length === 0 && (
        <div className="p-8 text-center text-sm text-muted-foreground">
          No tasks yet — add a task or leave to get started
        </div>
      )}

      {/* Data rows */}
      {sortedRows.map((row) => {
        const weeklyTotal = getWeeklyRowTotal(row.rowId);
        const rowHours = getRowHours(row.rowId);
        const rowCellRefs = cellRefsMap.current.get(row.rowId);
        const isLeave = row.kind === 'leave';

        return (
          <div key={row.rowId} className="grid grid-cols-[12rem_repeat(7,1fr)_4rem] border-b last:border-b-0">
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
                  dailyOtherTotal={getDailyOtherTotal(row.rowId, index)}
                  isWeekend={false}
                  isLeave={isLeave}
                  onCommit={(value) => handleHourCommit(row.rowId, index, value)}
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

      {/* Add task / add leave buttons */}
      {!isSubmitted && (
        <div className="flex items-center gap-2 border-t p-2">
          <TaskPickerPopover availableTasks={availableTasks} onPick={handlePickTask} />
          <LeavePickerPopover availableLeaveTypes={availableLeaveTypes} onPick={handlePickLeave} />
        </div>
      )}
    </div>
  );
});
