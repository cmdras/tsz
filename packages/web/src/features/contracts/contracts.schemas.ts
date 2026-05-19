import { z } from 'zod';
import type { ContractSort } from './contracts.server';

export const contractTaskSchema = z.object({
  id: z.string().uuid().optional(),
  name: z.string().trim().min(1, 'Name is required'),
  dayRate: z.number().positive('Day rate must be greater than 0'),
});

export type ContractTaskInput = z.infer<typeof contractTaskSchema>;

export const contractSchema = z
  .object({
    customerId: z.string().uuid('Customer is required'),
    consultantId: z.string().uuid('Consultant is required'),
    subject: z.string().trim().min(1, 'Subject is required'),
    startDate: z.string().min(1, 'Start date is required'),
    endDate: z.string(),
    tasks: z.array(contractTaskSchema).min(1, 'At least one task is required'),
  })
  .refine((data) => !data.endDate || data.startDate <= data.endDate, {
    message: 'End date must be on or after start date',
    path: ['endDate'],
  });

export type ContractInput = z.infer<typeof contractSchema>;

export const sortSlugs = {
  number: 'Number',
  customer: 'Customer',
  subject: 'Subject',
  consultant: 'Consultant',
  startdate: 'StartDate',
  enddate: 'EndDate',
} as const satisfies Record<string, ContractSort>;

export type SortSlug = keyof typeof sortSlugs;

const sortSlugValues = Object.keys(sortSlugs) as SortSlug[];
// oxlint-disable-next-line security/detect-non-literal-regexp
const sortPattern = new RegExp(`^(${sortSlugValues.join('|')})-?$`);

export const searchSchema = z.object({
  search: z.string().optional(),
  sort: z.string().regex(sortPattern).optional(),
  page: z.coerce.number().int().positive().optional(),
  archived: z.boolean().optional(),
});

export type SearchInput = z.infer<typeof searchSchema>;
