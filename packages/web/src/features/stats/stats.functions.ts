import { createServerFn } from '@tanstack/react-start';
import { getAdminStats } from './stats.server';

export const fetchAdminStats = createServerFn({ method: 'GET' }).handler(async () => {
  return await getAdminStats();
});
