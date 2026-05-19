import { createServerFn } from '@tanstack/react-start';
import { z } from 'zod';
import {
  getLeaveTypes,
  getLeaveTypeById,
  createLeaveType,
  updateLeaveType,
  archiveLeaveType,
  unarchiveLeaveType,
} from './leave-types.server';
import { ApiRequestError } from '#/api/client';
import { leaveTypeSchema, searchSchema, sortSlugs, type SortSlug } from './leave-types.schemas';

export const fetchLeaveTypes = createServerFn({ method: 'GET' })
  .inputValidator(searchSchema)
  .handler(async ({ data }) => {
    const { sort, archived, ...rest } = data;
    if (!sort) return await getLeaveTypes({ ...rest, showArchived: archived });
    const isDesc = sort.endsWith('-');
    const slug = (isDesc ? sort.slice(0, -1) : sort) as SortSlug;
    return await getLeaveTypes({
      ...rest,
      sort: sortSlugs[slug],
      sortDirection: isDesc ? 'Desc' : 'Asc',
      showArchived: archived,
    });
  });

export const fetchLeaveTypeById = createServerFn({ method: 'GET' })
  .inputValidator(z.string().uuid())
  .handler(async ({ data: id }) => {
    try {
      return await getLeaveTypeById(id);
    } catch (error) {
      if (error instanceof ApiRequestError && error.status === 404) return null;
      throw error;
    }
  });

export const createLeaveTypeFn = createServerFn({ method: 'POST' })
  .inputValidator(leaveTypeSchema)
  .handler(async ({ data }) => {
    try {
      return await createLeaveType(data);
    } catch (error) {
      if (error instanceof ApiRequestError && error.status === 409) {
        throw new Error('A leave type with this name already exists.', { cause: error });
      }
      throw error;
    }
  });

const updateLeaveTypeSchema = z.object({
  id: z.string().uuid(),
  data: leaveTypeSchema,
});

export const updateLeaveTypeFn = createServerFn({ method: 'POST' })
  .inputValidator(updateLeaveTypeSchema)
  .handler(async ({ data: { id, data } }) => {
    try {
      return await updateLeaveType(id, data);
    } catch (error) {
      if (error instanceof ApiRequestError && error.status === 409) {
        throw new Error('A leave type with this name already exists.', { cause: error });
      }
      throw error;
    }
  });

export const archiveLeaveTypeFn = createServerFn({ method: 'POST' })
  .inputValidator(z.string().uuid())
  .handler(async ({ data: id }) => {
    await archiveLeaveType(id);
  });

export const unarchiveLeaveTypeFn = createServerFn({ method: 'POST' })
  .inputValidator(z.string().uuid())
  .handler(async ({ data: id }) => {
    await unarchiveLeaveType(id);
  });
