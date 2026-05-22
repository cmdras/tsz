import { createServerFn } from '@tanstack/react-start';
import { z } from 'zod';
import {
  getCustomers,
  getCustomerById,
  createCustomer,
  updateCustomer,
  archiveCustomer,
  unarchiveCustomer,
} from './customers.server';
import { ApiRequestError } from '#/api/client';
import { customerSchema, customerSearchSchema, archivedFilterMap } from './customers.schemas';

export const fetchCustomers = createServerFn({ method: 'GET' })
  .inputValidator(customerSearchSchema)
  .handler(async ({ data }) => {
    const { filter, ...rest } = data;
    return await getCustomers({ ...rest, archived: archivedFilterMap[filter ?? 'all'] });
  });

export const fetchCustomerById = createServerFn({ method: 'GET' })
  .inputValidator(z.string().uuid())
  .handler(async ({ data: id }) => {
    try {
      return await getCustomerById(id);
    } catch (error) {
      if (error instanceof ApiRequestError && error.status === 404) return null;
      throw error;
    }
  });

export const createCustomerFn = createServerFn({ method: 'POST' })
  .inputValidator(customerSchema)
  .handler(async ({ data }) => {
    return await createCustomer(data);
  });

const updateCustomerSchema = z.object({
  id: z.string().uuid(),
  data: customerSchema,
});

export const updateCustomerFn = createServerFn({ method: 'POST' })
  .inputValidator(updateCustomerSchema)
  .handler(async ({ data: { id, data } }) => {
    return await updateCustomer(id, data);
  });

export const archiveCustomerFn = createServerFn({ method: 'POST' })
  .inputValidator(z.string().uuid())
  .handler(async ({ data: id }) => {
    await archiveCustomer(id);
  });

export const unarchiveCustomerFn = createServerFn({ method: 'POST' })
  .inputValidator(z.string().uuid())
  .handler(async ({ data: id }) => {
    await unarchiveCustomer(id);
  });
