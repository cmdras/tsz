import { type paths } from '../schema';
import createClient from 'openapi-fetch';

const client = createClient<paths>({ baseUrl: 'http://localhost:5204' });

export const getAnimalById = async (id: number) => {
  return client.GET('/api/animals/{id}', {
    params: {
      path: { id },
    },
  });
};
