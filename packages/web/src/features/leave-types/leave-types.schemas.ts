import { z } from 'zod';
import type { LeaveTypeSort } from './leave-types.server';

export const leaveTypeSchema = z.object({
  name: z.string().trim().min(1, 'Name is required').max(100),
  defaultDays: z
    .number()
    .min(0, 'DefaultDays cannot be negative')
    .max(365, 'DefaultDays cannot exceed 365')
    .refine((value) => /^\d+(\.\d)?$/.test(String(value)), 'DefaultDays accepts at most one decimal place'),
});

export type LeaveTypeInput = z.infer<typeof leaveTypeSchema>;

export const sortSlugs = {
  name: 'Name',
  defaultdays: 'DefaultDays',
} as const satisfies Record<string, LeaveTypeSort>;

export type SortSlug = keyof typeof sortSlugs;

const sortSlugValues = Object.keys(sortSlugs) as SortSlug[];
const sortPattern = new RegExp(`^(${sortSlugValues.join('|')})-?$`);

export const searchSchema = z.object({
  search: z.string().optional(),
  sort: z.string().regex(sortPattern).optional(),
  page: z.coerce.number().int().positive().optional(),
  archived: z.boolean().optional(),
});

export type SearchInput = z.infer<typeof searchSchema>;
