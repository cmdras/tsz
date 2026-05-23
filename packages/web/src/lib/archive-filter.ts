import { z } from 'zod';

const archiveFilterValues = ['all', 'active', 'archived'] as const;
export type ArchiveFilter = (typeof archiveFilterValues)[number];
export const archiveFilterSchema = z.enum(archiveFilterValues);

export const archiveFilterApiMap = {
  all: 'All',
  active: 'Active',
  archived: 'Archived',
} as const satisfies Record<ArchiveFilter, 'Active' | 'All' | 'Archived'>;
