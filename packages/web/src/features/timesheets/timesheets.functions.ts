import { createServerFn } from '@tanstack/react-start';
import { z } from 'zod';
import { getMonth } from './timesheets.server';

export const fetchMonth = createServerFn({ method: 'GET' })
  .inputValidator(z.object({ month: z.string() }))
  .handler(async ({ data }) => {
    return await getMonth(data.month);
  });
