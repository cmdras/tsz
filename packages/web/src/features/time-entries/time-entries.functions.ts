import { createServerFn } from '@tanstack/react-start';
import { z } from 'zod';
import { getWeek } from './time-entries.server';

export const fetchWeek = createServerFn({ method: 'GET' })
  .inputValidator(z.object({ week: z.string() }))
  .handler(async ({ data }) => {
    return await getWeek(data.week);
  });
