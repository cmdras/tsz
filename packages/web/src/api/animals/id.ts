import { type paths } from '../schema';
import createClient from 'openapi-fetch';

const client = createClient<paths>({ baseUrl: '/' });

export const getAnimalById = async (id: number) => {
  return client.GET('/api/animals/{id}', {
    params: {
      path: { id },
    },
  });
};
