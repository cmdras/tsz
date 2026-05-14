import { createServerFn } from '@tanstack/react-start';
import { z } from 'zod';
import { getCustomers, getCustomerById, createCustomer, updateCustomer, archiveCustomer } from '#/api/customers';
import { ApiRequestError } from '#/api/client';
import { customerSchema, searchSchema, sortSlugs, type SortSlug } from './-schemas';

export const fetchCustomers = createServerFn({ method: 'GET' })
  .inputValidator(searchSchema)
  .handler(async ({ data }) => {
    const { sort, ...rest } = data;
    if (!sort) return await getCustomers(rest);
    const isDesc = sort.endsWith('-');
    const slug = (isDesc ? sort.slice(0, -1) : sort) as SortSlug;
    return await getCustomers({
      ...rest,
      sort: sortSlugs[slug],
      sortDirection: isDesc ? 'Desc' : 'Asc',
    });
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
