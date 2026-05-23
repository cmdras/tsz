import type { ReactNode } from 'react';
import { Link } from '@tanstack/react-router';
import { AdminSplitPageLayout } from '#/components/admin-split-page-layout';
import { Button } from '#/components/ui/button';
import type { User } from '#/features/users/users.server';
import type { ArchiveFilter } from '#/lib/archive-filter';
import { UserListPanel } from './user-list-panel';

interface UsersPageLayoutProps {
  users: User[];
  selectedId?: string;
  search?: string;
  filter?: ArchiveFilter;
  children: ReactNode;
}

export function UsersPageLayout({ users, selectedId, search, filter, children }: UsersPageLayoutProps) {
  return (
    <AdminSplitPageLayout
      title="Users"
      newButton={
        <Button asChild>
          <Link to="/admin/users/new">New user</Link>
        </Button>
      }
      list={<UserListPanel users={users} selectedId={selectedId} search={search} filter={filter} />}
      detailKey={selectedId}
    >
      {children}
    </AdminSplitPageLayout>
  );
}
