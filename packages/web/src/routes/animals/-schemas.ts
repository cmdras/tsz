import { z } from 'zod';

export const updateAnimalSchema = z.object({
  name: z.string().trim().min(1, 'Name is required'),
  species: z.string().trim().min(1, 'Species is required'),
  age: z.number().int().min(0, 'Age must be 0 or greater'),
});

export type UpdateAnimalInput = z.infer<typeof updateAnimalSchema>;

export const saveAnimalSchema = z.object({
  id: z.number().int(),
  animal: updateAnimalSchema,
});
