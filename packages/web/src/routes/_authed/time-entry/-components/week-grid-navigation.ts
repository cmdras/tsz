import { createRef, useEffect, useRef, useState } from 'react';
import type { HourCellHandle } from './hour-cell';
import { DAYS_COUNT, type WeekRowResponse } from './week-grid-model';

export function getNextWeekdayIndex(dayIndex: number): number | null {
  return dayIndex < 4 ? dayIndex + 1 : null;
}

export function getPrevWeekdayIndex(dayIndex: number): number | null {
  return dayIndex > 0 ? dayIndex - 1 : null;
}

function makeRowRefs(): React.RefObject<HourCellHandle | null>[] {
  return Array.from({ length: DAYS_COUNT }, () => createRef<HourCellHandle | null>());
}

function initializeRefsMap(rows: WeekRowResponse[]): Map<string, React.RefObject<HourCellHandle | null>[]> {
  return new Map(
    rows
      .filter((row) => row.contractTaskId || row.leaveTypeId)
      .map((row) => [row.contractTaskId ?? row.leaveTypeId!, makeRowRefs()]),
  );
}

export function useWeekGridNavigation(initialRows: WeekRowResponse[]) {
  const cellRefsMap = useRef(initializeRefsMap(initialRows));
  const [pendingFocusId, setPendingFocusId] = useState<string | null>(null);

  useEffect(() => {
    if (pendingFocusId === null) return;
    cellRefsMap.current.get(pendingFocusId)?.[0]?.current?.triggerFocus();
    setPendingFocusId(null);
  }, [pendingFocusId]);

  function getRowRefs(rowId: string) {
    return cellRefsMap.current.get(rowId);
  }

  function addRowRefs(rowId: string) {
    cellRefsMap.current.set(rowId, makeRowRefs());
  }

  function replaceAllRefs(rows: WeekRowResponse[]) {
    cellRefsMap.current = initializeRefsMap(rows);
  }

  function scheduleFocus(rowId: string) {
    setPendingFocusId(rowId);
  }

  return { getRowRefs, addRowRefs, replaceAllRefs, scheduleFocus };
}
