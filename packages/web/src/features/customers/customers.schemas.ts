import { z } from 'zod';
import type { CustomerSort } from './customers.server';

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

export const sortSlugs = {
  number: 'Number',
  name: 'Name',
  contact: 'ContactName',
  city: 'City',
} as const satisfies Record<string, CustomerSort>;

export type SortSlug = keyof typeof sortSlugs;

const sortSlugValues = Object.keys(sortSlugs) as SortSlug[];
// oxlint-disable-next-line security/detect-non-literal-regexp
const sortPattern = new RegExp(`^(${sortSlugValues.join('|')})-?$`);

export const customerFilterValues = ['all', 'active', 'archived'] as const;
export type CustomerFilter = (typeof customerFilterValues)[number];

export const searchSchema = z.object({
  search: z.string().optional(),
  sort: z.string().regex(sortPattern).optional(),
  page: z.coerce.number().int().positive().optional(),
  filter: z.enum(customerFilterValues).optional(),
});

export type SearchInput = z.infer<typeof searchSchema>;
