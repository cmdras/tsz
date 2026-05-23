import type { ReactNode } from 'react';
import { Link } from '@tanstack/react-router';
import { AdminSplitPageLayout } from '#/components/admin-split-page-layout';
import { Button } from '#/components/ui/button';
import type { Contract } from '#/features/contracts/contracts.server';
import type { ArchiveFilter } from '#/lib/archive-filter';
import { ContractListPanel } from './contract-list-panel';

interface ContractsPageLayoutProps {
  contracts: Contract[];
  total: number;
  selectedId?: string;
  search?: string;
  page?: number;
  filter?: ArchiveFilter;
  children: ReactNode;
}

export function ContractsPageLayout({
  contracts,
  total,
  selectedId,
  search,
  page,
  filter,
  children,
}: ContractsPageLayoutProps) {
  return (
    <AdminSplitPageLayout
      title="Contracts"
      newButton={
        <Button asChild>
          <Link to="/admin/contracts/new">New contract</Link>
        </Button>
      }
      list={
        <ContractListPanel
          contracts={contracts}
          total={total}
          selectedId={selectedId}
          search={search}
          page={page}
          filter={filter}
        />
      }
      detailKey={selectedId}
    >
      {children}
    </AdminSplitPageLayout>
  );
}
