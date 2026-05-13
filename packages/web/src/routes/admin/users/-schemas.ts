import { z } from 'zod';

export const userRoles = ['Admin', 'ClientManager', 'User'] as const;
export type UserRole = (typeof userRoles)[number];

export const roleLabels: Record<UserRole, string> = {
  Admin: 'Admin',
  ClientManager: 'Client Manager',
  User: 'User',
};

export const userSchema = z.object({
  name: z.string().trim().min(1, 'Name is required'),
  email: z.string().email('Must be a valid email'),
  role: z.enum(userRoles, { message: 'Role is required' }),
});

export type UserInput = z.infer<typeof userSchema>;

export const sortColumns = ['Name', 'Email', 'Role'] as const;
export type SortColumn = (typeof sortColumns)[number];

export const searchSchema = z.object({
  search: z.string().optional(),
  sort: z.enum(sortColumns).optional(),
  sortDirection: z.enum(['Asc', 'Desc']).optional(),
  page: z.coerce.number().int().positive().optional(),
});

export type SearchInput = z.infer<typeof searchSchema>;
