import type { ReactNode } from 'react';
import { Link } from '@tanstack/react-router';
import { AdminSplitPageLayout } from '#/components/admin-split-page-layout';
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
    <AdminSplitPageLayout
      title="Customers"
      newButton={
        <Button asChild>
          <Link to="/admin/customers/new">New customer</Link>
        </Button>
      }
      list={<CustomerListPanel customers={customers} selectedId={selectedId} search={search} filter={filter} />}
      detailKey={selectedId}
    >
      {children}
    </AdminSplitPageLayout>
  );
}
