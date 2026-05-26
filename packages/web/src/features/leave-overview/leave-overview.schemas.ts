import { z } from 'zod';

export const leaveOverviewSearchSchema = z.object({
  year: z.coerce.number().int().min(2000).max(2100).optional(),
  focus: z.string().uuid().optional(),
});

export type LeaveOverviewSearch = z.infer<typeof leaveOverviewSearchSchema>;

export const leaveOverviewTypeItemSchema = z.object({
  id: z.string().uuid(),
  name: z.string(),
  mode: z.enum(['Limited', 'Unlimited']),
  allowance: z.number().nonnegative(),
  takenDays: z.number().nonnegative(),
});

export const leaveOverviewDayItemSchema = z.object({
  date: z.string().regex(/^\d{4}-\d{2}-\d{2}$/),
  leaveTypeIds: z.array(z.string().uuid()),
});

export const leaveOverviewResponseSchema = z.object({
  year: z.number().int(),
  types: z.array(leaveOverviewTypeItemSchema),
  days: z.array(leaveOverviewDayItemSchema),
});

export type LeaveOverviewTypeItem = z.infer<typeof leaveOverviewTypeItemSchema>;
export type LeaveOverviewDayItem = z.infer<typeof leaveOverviewDayItemSchema>;
export type LeaveOverviewResponse = z.infer<typeof leaveOverviewResponseSchema>;
