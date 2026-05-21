import { z } from 'zod';

export const weekSearchSchema = z.object({
  week: z
    .string()
    .regex(/^\d{4}-\d{2}-\d{2}$/)
    .optional(),
});

export type WeekSearch = z.infer<typeof weekSearchSchema>;
