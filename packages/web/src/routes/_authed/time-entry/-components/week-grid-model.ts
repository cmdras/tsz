import type { PickerOptions, WeekCell, WeekRowResponse } from '#/features/time-entries/time-entries.server';
export type { WeekRowResponse };

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
