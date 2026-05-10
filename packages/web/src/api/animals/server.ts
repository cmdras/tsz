import { createServerFn } from '@tanstack/react-start';
import { z } from 'zod';
import { getAnimals, updateAnimal } from './index';
import { getAnimalById } from './id';
import { saveAnimalSchema } from './schemas';

export const fetchAnimals = createServerFn({ method: 'GET' }).handler(async () => {
  const { data } = await getAnimals();
  return data ?? [];
});

export const fetchAnimalById = createServerFn({ method: 'GET' })
  .inputValidator(z.number().int())
  .handler(async ({ data: id }) => {
    const { data } = await getAnimalById(id);
    return data ?? null;
  });

export const saveAnimal = createServerFn({ method: 'POST' })
  .inputValidator(saveAnimalSchema)
  .handler(async ({ data: { id, animal } }) => {
    await updateAnimal(id, animal);
  });
