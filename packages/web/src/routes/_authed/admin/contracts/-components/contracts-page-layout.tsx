import type { ReactNode } from 'react';
import { Link } from '@tanstack/react-router';
import { Button } from '#/components/ui/button';
import type { Contract } from '#/features/contracts/contracts.server';
import { ContractListPanel } from './contract-list-panel';

interface ContractsPageLayoutProps {
  contracts: Contract[];
  total: number;
  selectedId?: string;
  search?: string;
  page?: number;
  archived?: boolean;
  children: ReactNode;
}

export function ContractsPageLayout({
  contracts,
  total,
  selectedId,
  search,
  page,
  archived,
  children,
}: ContractsPageLayoutProps) {
  return (
    <div className="flex flex-col gap-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Contracts</h1>
        <Button asChild>
          <Link to="/admin/contracts/new">New contract</Link>
        </Button>
      </div>
      <div className="flex gap-4 h-[calc(100vh-10rem)]">
        <ContractListPanel
          contracts={contracts}
          total={total}
          selectedId={selectedId}
          search={search}
          page={page}
          archived={archived}
        />
        <div key={selectedId ?? 'empty'} className="flex-1 border rounded-lg overflow-y-auto animate-fade-in">
          {children}
        </div>
      </div>
    </div>
  );
}
