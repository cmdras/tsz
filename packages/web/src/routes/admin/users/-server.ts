import { createServerFn } from '@tanstack/react-start';
import { z } from 'zod';
import { getUsers, getUserById, createUser, updateUser, archiveUser } from '#/api/users';
import { ApiRequestError } from '#/api/client';
import { userSchema, searchSchema } from './-schemas';

export const fetchUsers = createServerFn({ method: 'GET' })
  .inputValidator(searchSchema)
  .handler(async ({ data }) => {
    return await getUsers(data);
  });

export const fetchUserById = createServerFn({ method: 'GET' })
  .inputValidator(z.string().uuid())
  .handler(async ({ data: id }) => {
    try {
      return await getUserById(id);
    } catch (error) {
      if (error instanceof ApiRequestError && error.status === 404) return null;
      throw error;
    }
  });

export const createUserFn = createServerFn({ method: 'POST' })
  .inputValidator(userSchema)
  .handler(async ({ data }) => {
    try {
      return await createUser(data);
    } catch (error) {
      if (error instanceof ApiRequestError && error.status === 409) {
        throw new Error('EMAIL_ALREADY_IN_USE');
      }
      throw error;
    }
  });

const updateUserSchema = z.object({
  id: z.string().uuid(),
  data: userSchema,
});

export const updateUserFn = createServerFn({ method: 'POST' })
  .inputValidator(updateUserSchema)
  .handler(async ({ data: { id, data } }) => {
    try {
      return await updateUser(id, data);
    } catch (error) {
      if (error instanceof ApiRequestError && error.status === 409) {
        throw new Error('EMAIL_ALREADY_IN_USE');
      }
      throw error;
    }
  });

export const archiveUserFn = createServerFn({ method: 'POST' })
  .inputValidator(z.string().uuid())
  .handler(async ({ data: id }) => {
    await archiveUser(id);
  });
