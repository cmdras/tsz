import { z } from 'zod';

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

export const sortColumns = ['Number', 'Name', 'ContactName', 'City'] as const;
export type SortColumn = (typeof sortColumns)[number];

export const searchSchema = z.object({
  search: z.string().optional(),
  sort: z.enum(sortColumns).optional(),
  dir: z.enum(['Asc', 'Desc']).optional(),
  page: z.coerce.number().int().positive().optional(),
});

export type SearchInput = z.infer<typeof searchSchema>;
