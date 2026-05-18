import { type components } from '#/api/schema';
import { client } from '#/api/client';

export type User = components['schemas']['User'];
export type UserRequest = components['schemas']['UserRequest'];
export type UserResponse = components['schemas']['UserResponse'];
export type UserLeaveAllowanceResponse = components['schemas']['UserLeaveAllowanceResponse'];
export type UserLeaveAllowanceRequest = components['schemas']['UserLeaveAllowanceRequest'];
export type PagedUsers = components['schemas']['PagedUsers'];
export type UserSort = NonNullable<components['schemas']['UserSort']>;
export type SortDirection = NonNullable<components['schemas']['SortDirection']>;
export type UserRole = NonNullable<components['schemas']['UserRole']>;

export interface ListUsersParams {
  search?: string;
  sort?: UserSort;
  sortDirection?: SortDirection;
  page?: number;
  pageSize?: number;
}

export const getUsers = async (params: ListUsersParams = {}): Promise<PagedUsers> => {
  const response = await client.GET('/api/users', {
    params: { query: params },
  });
  return response.data!;
};

export const getUserById = async (id: string): Promise<UserResponse> => {
  const response = await client.GET('/api/users/{id}', { params: { path: { id } } });
  return response.data!;
};

export const createUser = async (body: UserRequest): Promise<User> => {
  const response = await client.POST('/api/users', { body });
  return response.data!;
};

export const updateUser = async (id: string, body: UserRequest): Promise<User> => {
  const response = await client.PUT('/api/users/{id}', { params: { path: { id } }, body });
  return response.data!;
};

export const archiveUser = async (id: string): Promise<void> => {
  await client.PATCH('/api/users/{id}/archive', { params: { path: { id } } });
};

export const unarchiveUser = async (id: string): Promise<void> => {
  await client.PATCH('/api/users/{id}/unarchive', { params: { path: { id } } });
};
