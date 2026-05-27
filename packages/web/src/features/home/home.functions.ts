import { createServerFn } from '@tanstack/react-start';
import { z } from 'zod';
import { getMonth } from './home.server';

export const fetchHomeMonth = createServerFn({ method: 'GET' })
  .inputValidator(z.object({ yearMonth: z.string().regex(/^\d{4}-\d{2}$/) }))
  .handler(async ({ data }) => {
    return await getMonth(data.yearMonth);
  });
