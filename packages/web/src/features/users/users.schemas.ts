import { z } from 'zod';
import { allowanceModeSchema, daysWithOneDecimal } from '#/features/leave-types/leave-types.schemas';
import type { UserSort } from './users.server';

export const userRoles = ['Admin', 'ClientManager', 'User'] as const;
export type UserRole = (typeof userRoles)[number];

export const roleLabels: Record<UserRole, string> = {
  Admin: 'Admin',
  ClientManager: 'Client Manager',
  User: 'User',
};

export const userLeaveAllowanceSchema = z.object({
  id: z.string().uuid().nullish(),
  leaveTypeId: z.string().uuid(),
  mode: allowanceModeSchema,
  totalDays: daysWithOneDecimal,
});

export type UserLeaveAllowanceInput = z.infer<typeof userLeaveAllowanceSchema>;

export const userSchema = z.object({
  name: z.string().trim().min(1, 'Name is required'),
  email: z.string().email('Must be a valid email'),
  role: z.enum(userRoles, { message: 'Role is required' }),
  leaves: z.array(userLeaveAllowanceSchema),
});

export type UserInput = z.infer<typeof userSchema>;

export const sortSlugs = {
  name: 'Name',
  email: 'Email',
  role: 'Role',
} as const satisfies Record<string, UserSort>;

export type SortSlug = keyof typeof sortSlugs;

const sortSlugValues = Object.keys(sortSlugs) as SortSlug[];
const sortPattern = new RegExp(`^(${sortSlugValues.join('|')})-?$`);

export const searchSchema = z.object({
  search: z.string().optional(),
  sort: z.string().regex(sortPattern).optional(),
  page: z.coerce.number().int().positive().optional(),
});

export type SearchInput = z.infer<typeof searchSchema>;
