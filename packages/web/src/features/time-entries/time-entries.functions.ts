import { createServerFn } from '@tanstack/react-start';
import { z } from 'zod';
import { getWeek, getPickerOptions, saveWeekDraft, submitWeek } from './time-entries.server';
import type { WeekCell } from './time-entries.server';

export const fetchWeek = createServerFn({ method: 'GET' })
  .inputValidator(z.object({ week: z.string() }))
  .handler(async ({ data }) => {
    return await getWeek(data.week);
  });

export const fetchPickerOptions = createServerFn({ method: 'GET' })
  .inputValidator(z.object({ week: z.string() }))
  .handler(async ({ data }) => {
    return await getPickerOptions(data.week);
  });

const weekCellSchema = z.object({
  contractTaskId: z.string().uuid().nullable(),
  leaveTypeId: z.string().uuid().nullable(),
  date: z.string(),
  hours: z.number(),
});

const weekMutationSchema = z.object({
  week: z.string(),
  cells: z.array(weekCellSchema),
});

export const saveDraft = createServerFn({ method: 'POST' })
  .inputValidator(weekMutationSchema)
  .handler(async ({ data }) => {
    return await saveWeekDraft(data.week, data.cells as WeekCell[]);
  });

export const submitWeekFn = createServerFn({ method: 'POST' })
  .inputValidator(weekMutationSchema)
  .handler(async ({ data }) => {
    return await submitWeek(data.week, data.cells as WeekCell[]);
  });
