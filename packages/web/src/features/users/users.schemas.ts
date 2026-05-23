import { z } from 'zod';
import { allowanceModeSchema, daysWithOneDecimal } from '#/features/leave-types/leave-types.schemas';

export const userRoles = ['Admin', 'ClientManager', 'User'] as const;
export type UserRole = (typeof userRoles)[number];

export const roleLabels: Record<UserRole, string> = {
  Admin: 'Admin',
  ClientManager: 'Client Manager',
  User: 'User',
};

const userLeaveAllowanceSchema = z.object({
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

import { archiveFilterSchema } from '#/lib/archive-filter';

export const userSearchSchema = z.object({
  search: z.string().optional(),
  filter: archiveFilterSchema.optional(),
});

export type UserSearchInput = z.infer<typeof userSearchSchema>;
