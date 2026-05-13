import { type components } from '../schema';
import { client } from '../client';

export type Customer = components['schemas']['Customer'];
export type CustomerRequest = components['schemas']['CustomerRequest'];
export type PagedCustomers = components['schemas']['PagedCustomers'];
export type CustomerSort = NonNullable<components['schemas']['CustomerSort']>;
export type SortDirection = NonNullable<components['schemas']['SortDirection']>;

export interface ListCustomersParams {
  search?: string;
  sort?: CustomerSort;
  sortDirection?: SortDirection;
  page?: number;
  pageSize?: number;
}

export const getCustomers = async (params: ListCustomersParams = {}): Promise<PagedCustomers> => {
  const response = await client.GET('/api/customers', {
    params: { query: params },
  });
  return response.data!;
};

export const getCustomerById = async (id: string): Promise<Customer> => {
  const response = await client.GET('/api/customers/{id}', { params: { path: { id } } });
  return response.data!;
};

export const createCustomer = async (body: CustomerRequest): Promise<Customer> => {
  const response = await client.POST('/api/customers', { body });
  return response.data!;
};

export const updateCustomer = async (id: string, body: CustomerRequest): Promise<Customer> => {
  const response = await client.PUT('/api/customers/{id}', { params: { path: { id } }, body });
  return response.data!;
};

export const archiveCustomer = async (id: string): Promise<void> => {
  await client.PATCH('/api/customers/{id}/archive', { params: { path: { id } } });
};
