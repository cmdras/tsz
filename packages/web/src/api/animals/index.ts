import { type components } from '../schema';
import { client } from '../client';

export type AnimalDTO = components['schemas']['Animal'];
export type CreateAnimalRequestDTO = components['schemas']['CreateAnimalRequest'];
export type UpdateAnimalRequestDTO = components['schemas']['UpdateAnimalRequest'];

export const getAnimals = async (): Promise<AnimalDTO[]> => {
  const resp = await client.GET('/api/animals');
  return resp.data!;
};

export const getAnimalById = async (id: number): Promise<AnimalDTO> => {
  const resp = await client.GET('/api/animals/{id}', { params: { path: { id } } });
  return resp.data!;
};

export const createAnimal = async (animal: CreateAnimalRequestDTO): Promise<void> => {
  await client.POST('/api/animals', { body: animal });
};

export const updateAnimal = async (id: number, animal: UpdateAnimalRequestDTO): Promise<void> => {
  await client.PUT('/api/animals/{id}', { params: { path: { id } }, body: animal });
};

export const removeAnimal = async (id: number): Promise<void> => {
  await client.DELETE('/api/animals/{id}', { params: { path: { id } } });
};
