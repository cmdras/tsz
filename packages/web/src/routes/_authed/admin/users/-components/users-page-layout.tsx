import type { ReactNode } from 'react';
import { Link } from '@tanstack/react-router';
import { AdminSplitPageLayout } from '#/components/admin-split-page-layout';
import { Button } from '#/components/ui/button';
import type { User } from '#/features/users/users.server';
import { UserListPanel } from './user-list-panel';

interface UsersPageLayoutProps {
  users: User[];
  selectedId?: string;
  search?: string;
  children: ReactNode;
}

export function UsersPageLayout({ users, selectedId, search, children }: UsersPageLayoutProps) {
  return (
    <AdminSplitPageLayout
      title="Users"
      newButton={
        <Button asChild>
          <Link to="/admin/users/new">New user</Link>
        </Button>
      }
      list={<UserListPanel users={users} selectedId={selectedId} search={search} />}
      detailKey={selectedId}
    >
      {children}
    </AdminSplitPageLayout>
  );
}
