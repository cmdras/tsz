import { createServerFn } from '@tanstack/react-start';
import { z } from 'zod';
import { getMonth } from '#/features/timesheets/timesheets.server';

export const fetchHomeMonth = createServerFn({ method: 'GET' })
  .inputValidator(z.object({ yearMonth: z.string().regex(/^\d{4}-(0[1-9]|1[0-2])$/) }))
  .handler(async ({ data }) => {
    return await getMonth(data.yearMonth);
  });
