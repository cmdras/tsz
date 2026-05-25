import { addDays, toIsoDateString } from '#/lib/date-utils';
import type { PickerOptions, WeekCell, WeekRowResponse } from '#/features/time-entries/time-entries.server';
export type { WeekRowResponse };

export const DAYS_COUNT = 7;

export type TaskGridRow = {
  kind: 'task';
  rowId: string;
  contractTaskId: string;
  customerName: string;
  contractSubject: string;
  taskName: string;
};

export type LeaveGridRow = {
  kind: 'leave';
  rowId: string;
  leaveTypeId: string;
  leaveTypeName: string;
};

export type GridRow = TaskGridRow | LeaveGridRow;

export interface WeekGridHandle {
  getCells: () => WeekCell[];
  resetDirty: () => void;
  hasRows: () => boolean;
  loadWeek: (rows: WeekRowResponse[]) => void;
}

export interface WeekGridProps {
  weekStart: string;
  savedRows: WeekRowResponse[];
  pickerOptions: PickerOptions;
  isSubmitted: boolean;
  onDirtyChange: (dirty: boolean) => void;
}

export function rowsFromSaved(savedRows: WeekRowResponse[]): GridRow[] {
  return savedRows.flatMap((row): GridRow[] => {
    if (row.contractTaskId) {
      return [
        {
          kind: 'task',
          rowId: row.contractTaskId,
          contractTaskId: row.contractTaskId,
          customerName: row.customerName ?? '',
          contractSubject: row.contractSubject ?? '',
          taskName: row.taskName ?? '',
        },
      ];
    }
    if (row.leaveTypeId) {
      return [
        {
          kind: 'leave',
          rowId: row.leaveTypeId,
          leaveTypeId: row.leaveTypeId,
          leaveTypeName: row.leaveTypeName ?? '',
        },
      ];
    }
    return [];
  });
}

export function hoursFromSaved(savedRows: WeekRowResponse[]): Record<string, (number | null)[]> {
  return Object.fromEntries(
    savedRows
      .filter((row) => row.contractTaskId || row.leaveTypeId)
      .map((row) => [row.contractTaskId ?? row.leaveTypeId!, row.hours as (number | null)[]]),
  );
}

export function sortGridRows(rows: GridRow[]): GridRow[] {
  return [...rows].toSorted((rowA, rowB) => {
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
}

export function makeHourHelpers(hours: Record<string, (number | null)[]>, sortedRows: GridRow[]) {
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
  return { getRowHours, getDailyTotal, getDailyOtherTotal, getWeeklyRowTotal };
}

export function buildCells(rows: GridRow[], hours: Record<string, (number | null)[]>, monday: Date): WeekCell[] {
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
      return [{ contractTaskId: null, leaveTypeId: row.leaveTypeId, date: toIsoDateString(date), hours: hourValue }];
    });
  });
}

export function getAvailableOptions(rows: GridRow[], pickerOptions: PickerOptions) {
  const pickedTaskIds = new Set(rows.flatMap((row) => (row.kind === 'task' ? [row.contractTaskId] : [])));
  const pickedLeaveTypeIds = new Set(rows.flatMap((row) => (row.kind === 'leave' ? [row.leaveTypeId] : [])));
  return {
    availableTasks: pickerOptions.availableTasks.filter((task) => !pickedTaskIds.has(task.contractTaskId)),
    availableLeaveTypes: pickerOptions.availableLeaveTypes.filter(
      (leaveType) => !pickedLeaveTypeIds.has(leaveType.leaveTypeId),
    ),
  };
}
