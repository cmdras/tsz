import { forwardRef, useImperativeHandle, useRef, useState } from 'react';
import { fromIsoDateString, toIsoDateString } from '#/lib/date-utils';
import type { PickerLeaveTypeOption, PickerTaskOption } from '#/features/time-entries/time-entries.server';
import {
  buildCells,
  DAYS_COUNT,
  getAvailableOptions,
  type GridRow,
  type LeaveGridRow,
  type TaskGridRow,
  hoursFromSaved,
  makeHourHelpers,
  rowsFromSaved,
  sortGridRows,
} from './week-grid-model';
import type { WeekGridProps, WeekRowResponse } from './week-grid-model';
import { LeavePickerPopover, TaskPickerPopover } from './week-grid-pickers';
import { WeekDailyTotals, WeekHeader, WeekRow } from './week-grid-rows';
import { useWeekGridNavigation } from './week-grid-navigation';

export type { WeekGridHandle } from './week-grid-model';

export const WeekGrid = forwardRef<import('./week-grid-model').WeekGridHandle, WeekGridProps>(function WeekGrid(
  { weekStart, savedRows, pickerOptions, isSubmitted, onDirtyChange },
  ref,
) {
  const monday = fromIsoDateString(weekStart);
  const todayIso = toIsoDateString(new Date());
  const [rows, setRows] = useState<GridRow[]>(() => rowsFromSaved(savedRows));
  const [hours, setHours] = useState<Record<string, (number | null)[]>>(() => hoursFromSaved(savedRows));
  const { getRowRefs, addRowRefs, replaceAllRefs, scheduleFocus } = useWeekGridNavigation(savedRows);
  const isDirtyRef = useRef(false);

  useImperativeHandle(ref, () => ({
    resetDirty() {
      isDirtyRef.current = false;
    },
    hasRows() {
      return rows.length > 0;
    },
    loadWeek(newRows: WeekRowResponse[]) {
      replaceAllRefs(newRows);
      setRows(rowsFromSaved(newRows));
      setHours(hoursFromSaved(newRows));
      isDirtyRef.current = true;
      onDirtyChange(true);
    },
    getCells() {
      return buildCells(rows, hours, monday);
    },
  }));

  const sortedRows = sortGridRows(rows);
  const { availableTasks, availableLeaveTypes } = getAvailableOptions(rows, pickerOptions);
  const { getRowHours, getDailyTotal, getDailyOtherTotal, getWeeklyRowTotal } = makeHourHelpers(hours, sortedRows);

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
    addRowRefs(task.contractTaskId);
    scheduleFocus(task.contractTaskId);
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
    addRowRefs(leaveType.leaveTypeId);
    scheduleFocus(leaveType.leaveTypeId);
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
      <WeekHeader monday={monday} todayIso={todayIso} todayHasLeave={todayHasLeave} />

      {sortedRows.length === 0 && (
        <div className="p-8 text-center text-sm text-muted-foreground">
          No tasks yet — add a task or leave to get started
        </div>
      )}

      {sortedRows.map((row) => (
        <WeekRow
          key={row.rowId}
          row={row}
          rowHours={getRowHours(row.rowId)}
          rowCellRefs={getRowRefs(row.rowId)}
          isSubmitted={isSubmitted}
          getDailyOtherTotal={(dayIndex) => getDailyOtherTotal(row.rowId, dayIndex)}
          onHourCommit={(dayIndex, value) => handleHourCommit(row.rowId, dayIndex, value)}
          weeklyTotal={getWeeklyRowTotal(row.rowId)}
        />
      ))}

      <WeekDailyTotals sortedRowIds={sortedRows.map((row) => row.rowId)} getDailyTotal={getDailyTotal} />

      {!isSubmitted && (
        <div className="flex items-center gap-2 border-t p-2">
          <TaskPickerPopover availableTasks={availableTasks} onPick={handlePickTask} />
          <LeavePickerPopover availableLeaveTypes={availableLeaveTypes} onPick={handlePickLeave} />
        </div>
      )}
    </div>
  );
});
