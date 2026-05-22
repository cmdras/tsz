import { createServerFn } from '@tanstack/react-start';
import { z } from 'zod';
import { getUsers, getUserById, createUser, updateUser, archiveUser } from './users.server';
import { getLeaveTypes } from '#/features/leave-types/leave-types.server';
import { ApiRequestError } from '#/api/client';
import { userSchema, userSearchSchema } from './users.schemas';

export const fetchUsers = createServerFn({ method: 'GET' })
  .inputValidator(userSearchSchema)
  .handler(({ data }) => getUsers(data));

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
        throw new Error('EMAIL_ALREADY_IN_USE', { cause: error });
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
        if (error.body?.includes('leave allowance')) {
          throw new Error('DUPLICATE_LEAVE_ALLOWANCE', { cause: error });
        }
        throw new Error('EMAIL_ALREADY_IN_USE', { cause: error });
      }
      throw error;
    }
  });

export const archiveUserFn = createServerFn({ method: 'POST' })
  .inputValidator(z.string().uuid())
  .handler(async ({ data: id }) => {
    await archiveUser(id);
  });

export const listLeaveTypesForPickerFn = createServerFn({ method: 'GET' }).handler(async () => {
  const result = await getLeaveTypes({ pageSize: 100, showArchived: false });
  return result.items;
});
