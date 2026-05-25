import { type RefObject, useState } from 'react';
import { toast } from 'sonner';
import { fetchWeek } from '#/features/time-entries/time-entries.functions';
import type { PickerOptions, WeekRowResponse } from '#/features/time-entries/time-entries.server';
import { addDays, fromIsoDateString, toIsoDateString } from '#/lib/date-utils';
import type { WeekGridHandle } from './week-grid';

interface UseCopyLastWeekOptions {
  weekStart: string;
  weekRows: WeekRowResponse[];
  pickerOptions: PickerOptions;
  gridRef: RefObject<WeekGridHandle | null>;
}

export function useCopyLastWeek({ weekStart, weekRows, pickerOptions, gridRef }: UseCopyLastWeekOptions) {
  const [pendingCopyRows, setPendingCopyRows] = useState<WeekRowResponse[] | null>(null);

  async function handleCopyLastWeek() {
    const previousWeekStart = toIsoDateString(addDays(fromIsoDateString(weekStart), -7));
    let previousWeekData;
    try {
      previousWeekData = await fetchWeek({ data: { week: previousWeekStart } });
    } catch {
      toast.error('Failed to fetch last week.');
      return;
    }

    if (previousWeekData.rows.length === 0) {
      toast.info('No entries last week.');
      return;
    }

    const validTaskIds = new Set([
      ...weekRows.filter((row) => row.contractTaskId).map((row) => row.contractTaskId!),
      ...pickerOptions.availableTasks.map((task) => task.contractTaskId),
    ]);
    const validLeaveTypeIds = new Set([
      ...weekRows.filter((row) => row.leaveTypeId).map((row) => row.leaveTypeId!),
      ...pickerOptions.availableLeaveTypes.map((leaveType) => leaveType.leaveTypeId),
    ]);

    const filteredRows = previousWeekData.rows.filter((row) => {
      if (row.contractTaskId) return validTaskIds.has(row.contractTaskId);
      if (row.leaveTypeId) return validLeaveTypeIds.has(row.leaveTypeId);
      return false;
    });

    if (filteredRows.length === 0) {
      toast.info("All last week's entries are no longer available.");
      return;
    }

    if (gridRef.current?.hasRows()) {
      setPendingCopyRows(filteredRows);
    } else {
      gridRef.current?.loadWeek(filteredRows);
    }
  }

  function applyPendingCopy() {
    if (pendingCopyRows && gridRef.current) {
      gridRef.current.loadWeek(pendingCopyRows);
    }
    setPendingCopyRows(null);
  }

  return {
    handleCopyLastWeek,
    pendingCopyRows,
    applyPendingCopy,
    clearPendingCopy: () => setPendingCopyRows(null),
  };
}
