import { createFileRoute, Outlet } from '@tanstack/react-router';
import { fetchAllCustomers } from '#/features/customers/customers.functions';
import { searchSchema } from '#/features/customers/customers.schemas';
import { CustomersPageLayout } from './-components/customers-page-layout';

export const Route = createFileRoute('/_authed/admin/customers/$id')({
  validateSearch: searchSchema,
  loader: () => fetchAllCustomers(),
  staleTime: 30_000,
  component: CustomerDetailLayout,
});

function CustomerDetailLayout() {
  const { items } = Route.useLoaderData();
  const { id } = Route.useParams();
  const { search, filter } = Route.useSearch();

  return (
    <CustomersPageLayout customers={items} selectedId={id} search={search} filter={filter}>
      <Outlet />
    </CustomersPageLayout>
  );
}
