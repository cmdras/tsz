import { createServerFn } from '@tanstack/react-start';
import { z } from 'zod';
import {
  getContracts,
  getContractById,
  createContract,
  updateContract,
  archiveContract,
  unarchiveContract,
} from './contracts.server';
import { getCustomers } from '#/features/customers/customers.server';
import { getUsers } from '#/features/users/users.server';
import { ApiRequestError } from '#/api/client';
import { contractSchema, searchSchema, sortSlugs, type ContractInput, type SortSlug } from './contracts.schemas';

function toContractRequest(data: ContractInput) {
  return {
    customerId: data.customerId,
    consultantId: data.consultantId,
    subject: data.subject,
    startDate: data.startDate,
    endDate: data.endDate || null,
    tasks: data.tasks.map((task) => ({
      id: task.id ?? null,
      name: task.name,
      dayRate: task.dayRate,
    })),
  };
}

export const fetchContracts = createServerFn({ method: 'GET' })
  .inputValidator(searchSchema)
  .handler(async ({ data }) => {
    const { sort, archived, ...rest } = data;
    if (!sort) return await getContracts({ ...rest, archived });
    const isDesc = sort.endsWith('-');
    const slug = (isDesc ? sort.slice(0, -1) : sort) as SortSlug;
    return await getContracts({
      ...rest,
      sort: sortSlugs[slug],
      sortDirection: isDesc ? 'Desc' : 'Asc',
      archived,
    });
  });

export const fetchContractById = createServerFn({ method: 'GET' })
  .inputValidator(z.string().uuid())
  .handler(async ({ data: id }) => {
    try {
      return await getContractById(id);
    } catch (error) {
      if (error instanceof ApiRequestError && error.status === 404) return null;
      throw error;
    }
  });

export const fetchContractFormOptions = createServerFn({ method: 'GET' }).handler(async () => {
  const [customersResult, usersResult] = await Promise.all([
    getCustomers({ pageSize: 100 }),
    getUsers({ pageSize: 100 }),
  ]);
  const customers = customersResult.items;
  const consultants = usersResult.items.filter((user) => user.role !== 'ClientManager');
  return { customers, consultants };
});

export const createContractFn = createServerFn({ method: 'POST' })
  .inputValidator(contractSchema)
  .handler(async ({ data }) => {
    return await createContract(toContractRequest(data));
  });

const updateContractSchema = z.object({
  id: z.string().uuid(),
  data: contractSchema,
});

export const updateContractFn = createServerFn({ method: 'POST' })
  .inputValidator(updateContractSchema)
  .handler(async ({ data: { id, data } }) => {
    return await updateContract(id, toContractRequest(data));
  });

export const archiveContractFn = createServerFn({ method: 'POST' })
  .inputValidator(z.string().uuid())
  .handler(async ({ data: id }) => {
    await archiveContract(id);
  });

export const unarchiveContractFn = createServerFn({ method: 'POST' })
  .inputValidator(z.string().uuid())
  .handler(async ({ data: id }) => {
    await unarchiveContract(id);
  });
