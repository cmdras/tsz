import { type components } from '../schema';
import { client } from '../client';

export type CustomerDTO = components['schemas']['Customer'];
export type CustomerRequestDTO = components['schemas']['CustomerRequest'];

export const getCustomers = async (search?: string): Promise<CustomerDTO[]> => {
  const resp = await client.GET('/api/customers', {
    params: { query: { search } },
  });
  return resp.data!;
};

export const getCustomerById = async (id: string): Promise<CustomerDTO> => {
  const resp = await client.GET('/api/customers/{id}', { params: { path: { id } } });
  return resp.data!;
};

export const createCustomer = async (body: CustomerRequestDTO): Promise<CustomerDTO> => {
  const resp = await client.POST('/api/customers', { body });
  return resp.data!;
};

export const updateCustomer = async (id: string, body: CustomerRequestDTO): Promise<CustomerDTO> => {
  const resp = await client.PUT('/api/customers/{id}', { params: { path: { id } }, body });
  return resp.data!;
};

export const archiveCustomer = async (id: string): Promise<void> => {
  await client.PATCH('/api/customers/{id}/archive', { params: { path: { id } } });
};
