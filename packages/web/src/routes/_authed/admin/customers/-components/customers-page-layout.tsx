import type { ReactNode } from 'react';
import { Link } from '@tanstack/react-router';
import { Button } from '#/components/ui/button';
import type { Customer } from '#/features/customers/customers.server';
import type { CustomerFilter } from '#/features/customers/customers.schemas';
import { CustomerListPanel } from './customer-list-panel';

interface CustomersPageLayoutProps {
  customers: Customer[];
  selectedId?: string;
  search?: string;
  filter?: CustomerFilter;
  children: ReactNode;
}

export function CustomersPageLayout({ customers, selectedId, search, filter, children }: CustomersPageLayoutProps) {
  return (
    <div className="flex flex-col gap-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Customers</h1>
        <Button asChild>
          <Link to="/admin/customers/new">New customer</Link>
        </Button>
      </div>
      <div className="flex gap-4 h-[calc(100vh-10rem)]">
        <CustomerListPanel customers={customers} selectedId={selectedId} search={search} filter={filter} />
        <div key={selectedId ?? 'empty'} className="flex-1 border rounded-lg overflow-y-auto animate-fade-in">
          {children}
        </div>
      </div>
    </div>
  );
}
