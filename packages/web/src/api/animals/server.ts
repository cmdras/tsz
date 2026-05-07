import { createServerFn } from '@tanstack/react-start';
import { getAnimals, updateAnimal, type UpdateAnimalRequestDTO } from './index';
import { getAnimalById } from './id';

export const fetchAnimals = createServerFn({ method: 'GET' }).handler(async () => {
  const { data } = await getAnimals();
  return data ?? [];
});

export const fetchAnimalById = createServerFn({ method: 'GET' })
  .inputValidator((id: number) => id)
  .handler(async ({ data: id }) => {
    const { data } = await getAnimalById(id);
    return data ?? null;
  });

export const saveAnimal = createServerFn({ method: 'POST' })
  .inputValidator((input: { id: number; animal: UpdateAnimalRequestDTO }) => input)
  .handler(async ({ data: { id, animal } }) => {
    await updateAnimal(id, animal);
  });
