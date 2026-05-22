import { type components } from '#/api/schema';
import { client } from '#/api/client';

export type WeekResponse = components['schemas']['WeekResponse'];
export type WeekRowResponse = components['schemas']['WeekRowResponse'];
export type WeekCell = components['schemas']['WeekCell'];
export type PickerOptions = components['schemas']['PickerOptions'];
export type PickerTaskOption = components['schemas']['PickerTaskOption'];
export type PickerLeaveTypeOption = components['schemas']['PickerLeaveTypeOption'];

export const getWeek = async (weekStart: string): Promise<WeekResponse> => {
  const response = await client.GET('/api/time-entries/weeks/{weekStart}', {
    params: { path: { weekStart } },
  });
  return response.data!;
};

export const getPickerOptions = async (weekStart: string): Promise<PickerOptions> => {
  const response = await client.GET('/api/time-entries/pickers', {
    params: { query: { weekStart } },
  });
  return response.data!;
};

export const saveWeekDraft = async (weekStart: string, cells: WeekCell[]): Promise<WeekResponse> => {
  const response = await client.PUT('/api/time-entries/weeks/{weekStart}', {
    params: { path: { weekStart } },
    body: { cells },
  });
  return response.data!;
};
