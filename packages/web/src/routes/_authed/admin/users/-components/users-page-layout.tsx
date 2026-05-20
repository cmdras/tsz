import type { ReactNode } from 'react';
import { Link } from '@tanstack/react-router';
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
    <div className="flex flex-col gap-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Users</h1>
        <Button asChild>
          <Link to="/admin/users/new">New user</Link>
        </Button>
      </div>
      <div className="flex gap-4 h-[calc(100vh-10rem)]">
        <UserListPanel users={users} selectedId={selectedId} search={search} />
        <div key={selectedId ?? 'empty'} className="flex-1 border rounded-lg overflow-y-auto animate-fade-in">
          {children}
        </div>
      </div>
    </div>
  );
}
