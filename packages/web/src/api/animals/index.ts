import { type components } from '../schema';
import { client } from '../client';

export type Animal = components['schemas']['Animal'];
export type CreateAnimalRequest = components['schemas']['CreateAnimalRequest'];
export type UpdateAnimalRequest = components['schemas']['UpdateAnimalRequest'];

export const getAnimals = async (): Promise<Animal[]> => {
  const response = await client.GET('/api/animals');
  return response.data!;
};

export const getAnimalById = async (id: number): Promise<Animal> => {
  const response = await client.GET('/api/animals/{id}', { params: { path: { id } } });
  return response.data!;
};

export const createAnimal = async (animal: CreateAnimalRequest): Promise<void> => {
  await client.POST('/api/animals', { body: animal });
};

export const updateAnimal = async (id: number, animal: UpdateAnimalRequest): Promise<void> => {
  await client.PUT('/api/animals/{id}', { params: { path: { id } }, body: animal });
};

export const removeAnimal = async (id: number): Promise<void> => {
  await client.DELETE('/api/animals/{id}', { params: { path: { id } } });
};
