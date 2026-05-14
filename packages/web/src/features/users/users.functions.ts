import { createServerFn } from '@tanstack/react-start';
import { z } from 'zod';
import { getUsers, getUserById, createUser, updateUser, archiveUser } from './users.server';
import { ApiRequestError } from '#/api/client';
import { userSchema, searchSchema, sortSlugs, type SortSlug } from './users.schemas';

export const fetchUsers = createServerFn({ method: 'GET' })
  .inputValidator(searchSchema)
  .handler(async ({ data }) => {
    const { sort, ...rest } = data;
    if (!sort) return await getUsers(rest);
    const isDesc = sort.endsWith('-');
    const slug = (isDesc ? sort.slice(0, -1) : sort) as SortSlug;
    return await getUsers({
      ...rest,
      sort: sortSlugs[slug],
      sortDirection: isDesc ? 'Desc' : 'Asc',
    });
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
