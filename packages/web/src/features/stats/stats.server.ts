import { type components } from '#/api/schema';
import { client } from '#/api/client';

export type AdminStats = components['schemas']['AdminStats'];

export const getAdminStats = async (): Promise<AdminStats> => {
  const response = await client.GET('/api/stats/admin');
  return response.data!;
};
