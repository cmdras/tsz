import { type components } from '#/api/schema';
import { client } from '#/api/client';

export type LeaveOverviewResponse = components['schemas']['LeaveOverviewResponse'];
export type LeaveOverviewTypeItem = components['schemas']['LeaveOverviewTypeItem'];
export type LeaveOverviewDayItem = components['schemas']['LeaveOverviewDayItem'];

export const getLeaveOverview = async (year: number): Promise<LeaveOverviewResponse> => {
  const response = await client.GET('/api/leave-overview', {
    params: { query: { year } },
  });
  return response.data!;
};
