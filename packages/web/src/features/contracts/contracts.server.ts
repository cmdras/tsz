import { type components } from '#/api/schema';
import { client } from '#/api/client';

export type Contract = components['schemas']['Contract'];
export type ContractRequest = components['schemas']['ContractRequest'];
export type PagedContracts = components['schemas']['PagedContracts'];
export type ContractSort = NonNullable<components['schemas']['ContractSort']>;
export type ContractTask = components['schemas']['ContractTask'];
export type ContractTaskRequest = components['schemas']['ContractTaskRequest'];
export type SortDirection = NonNullable<components['schemas']['SortDirection']>;

export interface ListContractsParams {
  search?: string;
  sort?: ContractSort;
  sortDirection?: SortDirection;
  page?: number;
  pageSize?: number;
  archived?: boolean;
}

export const getContracts = async (params: ListContractsParams = {}): Promise<PagedContracts> => {
  const response = await client.GET('/api/contracts', {
    params: { query: params },
  });
  return response.data!;
};

export const getContractById = async (id: string): Promise<Contract> => {
  const response = await client.GET('/api/contracts/{id}', { params: { path: { id } } });
  return response.data!;
};

export const createContract = async (body: ContractRequest): Promise<Contract> => {
  const response = await client.POST('/api/contracts', { body });
  return response.data!;
};

export const updateContract = async (id: string, body: ContractRequest): Promise<Contract> => {
  const response = await client.PUT('/api/contracts/{id}', { params: { path: { id } }, body });
  return response.data!;
};

export const archiveContract = async (id: string): Promise<void> => {
  await client.PATCH('/api/contracts/{id}/archive', { params: { path: { id } } });
};

export const unarchiveContract = async (id: string): Promise<void> => {
  await client.PATCH('/api/contracts/{id}/unarchive', { params: { path: { id } } });
};
