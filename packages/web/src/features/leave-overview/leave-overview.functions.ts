import { createServerFn } from '@tanstack/react-start';
import { z } from 'zod';
import { getLeaveOverview } from './leave-overview.server';

export const fetchLeaveOverview = createServerFn({ method: 'GET' })
  .inputValidator(z.object({ year: z.number().int() }))
  .handler(async ({ data }) => {
    return await getLeaveOverview(data.year);
  });
