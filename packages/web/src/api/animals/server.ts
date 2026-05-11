import { createServerFn } from '@tanstack/react-start';
import { z } from 'zod';
import { getAnimals, getAnimalById, updateAnimal } from './index';
import { ApiRequestError } from '../client';
import { saveAnimalSchema } from './schemas';

export const fetchAnimals = createServerFn({ method: 'GET' }).handler(async () => {
  return await getAnimals();
});

export const fetchAnimalById = createServerFn({ method: 'GET' })
  .inputValidator(z.number().int())
  .handler(async ({ data: id }) => {
    try {
      return await getAnimalById(id);
    } catch (err) {
      if (err instanceof ApiRequestError && err.status === 404) return null;
      throw err;
    }
  });

export const saveAnimal = createServerFn({ method: 'POST' })
  .inputValidator(saveAnimalSchema)
  .handler(async ({ data: { id, animal } }) => {
    await updateAnimal(id, animal);
  });
