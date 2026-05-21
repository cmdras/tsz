import { type components } from '#/api/schema';
import { client } from '#/api/client';

export type WeekResponse = components['schemas']['WeekResponse'];

export const getWeek = async (weekStart: string): Promise<WeekResponse> => {
  const response = await client.GET('/api/time-entries/weeks/{weekStart}', {
    params: { path: { weekStart } },
  });
  return response.data!;
};
