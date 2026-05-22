import { z } from 'zod';
import type { ArchivedFilter } from './customers.server';

export const customerSchema = z.object({
  name: z.string().trim().min(1, 'Name is required'),
  street: z.string(),
  zip: z.string(),
  city: z.string(),
  country: z.string().min(1, 'Country is required'),
  contactName: z.string(),
  contactEmail: z.string().email('Must be a valid email').or(z.literal('')),
});

export type CustomerInput = z.infer<typeof customerSchema>;

export const customerFilterValues = ['all', 'active', 'archived'] as const;
export type CustomerFilter = (typeof customerFilterValues)[number];

export const archivedFilterMap: Record<CustomerFilter, ArchivedFilter> = {
  all: 'All',
  active: 'Active',
  archived: 'Archived',
};

export const customerSearchSchema = z.object({
  search: z.string().optional(),
  page: z.coerce.number().int().positive().optional(),
  filter: z.enum(customerFilterValues).optional(),
});

export type CustomerSearchInput = z.infer<typeof customerSearchSchema>;
