import type { ReactNode } from 'react';
import { Link } from '@tanstack/react-router';
import { AdminSplitPageLayout } from '#/components/admin-split-page-layout';
import { Button } from '#/components/ui/button';
import type { LeaveType } from '#/features/leave-types/leave-types.server';
import { LeaveTypeListPanel } from './leave-type-list-panel';

interface LeaveTypesPageLayoutProps {
  leaveTypes: LeaveType[];
  total: number;
  selectedId?: string;
  search?: string;
  page?: number;
  archived?: boolean;
  children: ReactNode;
}

export function LeaveTypesPageLayout({
  leaveTypes,
  total,
  selectedId,
  search,
  page,
  archived,
  children,
}: LeaveTypesPageLayoutProps) {
  return (
    <AdminSplitPageLayout
      title="Leave Types"
      newButton={
        <Button asChild>
          <Link to="/admin/leave-types/new">New leave type</Link>
        </Button>
      }
      list={
        <LeaveTypeListPanel
          leaveTypes={leaveTypes}
          total={total}
          selectedId={selectedId}
          search={search}
          page={page}
          archived={archived}
        />
      }
      detailKey={selectedId}
    >
      {children}
    </AdminSplitPageLayout>
  );
}
