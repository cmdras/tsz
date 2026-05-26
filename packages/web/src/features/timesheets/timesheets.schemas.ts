import { z } from 'zod';

export const monthSearchSchema = z.object({
  month: z
    .string()
    .refine(
      (value) => {
        if (value.length !== 7) return false;
        if (value[4] !== '-') return false;
        const year = parseInt(value.slice(0, 4), 10);
        const month = parseInt(value.slice(5, 7), 10);
        return !isNaN(year) && !isNaN(month) && month >= 1 && month <= 12;
      },
      { message: 'Must be in YYYY-MM format' },
    )
    .optional(),
});

export type MonthSearch = z.infer<typeof monthSearchSchema>;
