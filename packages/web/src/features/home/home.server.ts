import { type components } from '#/api/schema';
import { client } from '#/api/client';

export type MonthResponse = components['schemas']['MonthResponse'];

export const getMonth = async (yearMonth: string): Promise<MonthResponse> => {
  const response = await client.GET('/api/time-entries/months/{yearMonth}', {
    params: { path: { yearMonth } },
  });
  return response.data!;
};
