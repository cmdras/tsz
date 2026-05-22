import { createServerFn } from '@tanstack/react-start';
import { z } from 'zod';
import { getWeek, getPickerOptions, saveWeekDraft } from './time-entries.server';
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

export const saveDraft = createServerFn({ method: 'POST' })
  .inputValidator(
    z.object({
      week: z.string(),
      cells: z.array(
        z.object({
          contractTaskId: z.string().uuid().nullable(),
          leaveTypeId: z.string().uuid().nullable(),
          date: z.string(),
          hours: z.number(),
        }),
      ),
    }),
  )
  .handler(async ({ data }) => {
    return await saveWeekDraft(data.week, data.cells as WeekCell[]);
  });
