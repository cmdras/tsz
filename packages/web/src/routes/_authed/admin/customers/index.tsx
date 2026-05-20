import { createFileRoute } from '@tanstack/react-router';
import { fetchCustomers } from '#/features/customers/customers.functions';
import { searchSchema } from '#/features/customers/customers.schemas';
import { CustomerEmptyPanel } from './-components/customer-empty-panel';
import { CustomersPageLayout } from './-components/customers-page-layout';

export const Route = createFileRoute('/_authed/admin/customers/')({
  validateSearch: searchSchema,
  loaderDeps: ({ search }) => ({ search: search.search, filter: search.filter }),
  loader: ({ deps }) => fetchCustomers({ data: { search: deps.search, filter: deps.filter } }),
  staleTime: 30_000,
  component: CustomerList,
});

function CustomerList() {
  const { items } = Route.useLoaderData();
  const { search, filter } = Route.useSearch();

  return (
    <CustomersPageLayout customers={items} search={search} filter={filter}>
      <CustomerEmptyPanel />
    </CustomersPageLayout>
  );
}
