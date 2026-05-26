import { type components } from '#/api/schema';
import { client } from '#/api/client';

export type MonthResponse = components['schemas']['MonthResponse'];
export type MonthDayResponse = components['schemas']['MonthDayResponse'];
export type MonthEntryResponse = components['schemas']['MonthEntryResponse'];
export type WeekSubmissionStatusResponse = components['schemas']['WeekSubmissionStatusResponse'];

export const getMonth = async (yearMonth: string): Promise<MonthResponse> => {
  const response = await client.GET('/api/time-entries/months/{yearMonth}', {
    params: { path: { yearMonth } },
  });
  return response.data!;
};
