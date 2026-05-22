import { z } from 'zod';
import type { LeaveTypeSort } from './leave-types.server';

export const allowanceModes = ['Unlimited', 'Limited'] as const;
export type AllowanceMode = (typeof allowanceModes)[number];

export const allowanceModeSchema = z.enum(allowanceModes);

export const daysWithOneDecimal = z
  .number()
  .min(0, 'Cannot be negative')
  .max(365, 'Cannot exceed 365')
  .refine((value) => /^\d+(\.\d)?$/.test(String(value)), 'Accepts at most one decimal place');

export const leaveTypeSchema = z.object({
  name: z.string().trim().min(1, 'Name is required').max(100),
  defaultDays: daysWithOneDecimal,
  defaultMode: allowanceModeSchema,
});

export type LeaveTypeInput = z.infer<typeof leaveTypeSchema>;

export const sortSlugs = {
  name: 'Name',
  defaultdays: 'DefaultDays',
} as const satisfies Record<string, LeaveTypeSort>;

export type SortSlug = keyof typeof sortSlugs;

const sortSlugValues = Object.keys(sortSlugs) as SortSlug[];
// oxlint-disable-next-line security/detect-non-literal-regexp
const sortPattern = new RegExp(`^(${sortSlugValues.join('|')})-?$`);

export const PAGE_SIZE = 25;

export const leaveTypeSearchSchema = z.object({
  search: z.string().optional(),
  sort: z.string().regex(sortPattern).optional(),
  page: z.coerce.number().int().positive().optional(),
  archived: z.boolean().optional(),
});

export type LeaveTypeSearchInput = z.infer<typeof leaveTypeSearchSchema>;
