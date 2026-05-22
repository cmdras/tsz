import { type components } from '#/api/schema';
import { client } from '#/api/client';

export type LeaveType = components['schemas']['LeaveTypeResponse'];
export type LeaveTypeRequest = components['schemas']['LeaveTypeRequest'];
export type PagedLeaveTypes = components['schemas']['PagedLeaveTypes'];
export type LeaveTypeSort = NonNullable<components['schemas']['LeaveTypeSort']>;
type SortDirection = NonNullable<components['schemas']['SortDirection']>;

export interface ListLeaveTypesParams {
  search?: string;
  sort?: LeaveTypeSort;
  sortDirection?: SortDirection;
  page?: number;
  pageSize?: number;
  showArchived?: boolean;
}

export const getLeaveTypes = async (params: ListLeaveTypesParams = {}): Promise<PagedLeaveTypes> => {
  const response = await client.GET('/api/leave-types', {
    params: { query: params },
  });
  return response.data!;
};

export const getLeaveTypeById = async (id: string): Promise<LeaveType> => {
  const response = await client.GET('/api/leave-types/{id}', { params: { path: { id } } });
  return response.data!;
};

export const createLeaveType = async (body: LeaveTypeRequest): Promise<LeaveType> => {
  const response = await client.POST('/api/leave-types', { body });
  return response.data!;
};

export const updateLeaveType = async (id: string, body: LeaveTypeRequest): Promise<LeaveType> => {
  const response = await client.PUT('/api/leave-types/{id}', { params: { path: { id } }, body });
  return response.data!;
};

export const archiveLeaveType = async (id: string): Promise<void> => {
  await client.PATCH('/api/leave-types/{id}/archive', { params: { path: { id } } });
};

export const unarchiveLeaveType = async (id: string): Promise<void> => {
  await client.PATCH('/api/leave-types/{id}/unarchive', { params: { path: { id } } });
};
