import { createFileRoute, useRouter } from '@tanstack/react-router';
import { fetchCustomerById } from '#/features/customers/customers.functions';
import { CustomerDetailPanel } from '../-components/customer-detail-panel';
import { CustomerNotFound } from '../-components/customer-not-found';

export const Route = createFileRoute('/_authed/admin/customers/$id/')({
  loader: ({ params }) => fetchCustomerById({ data: params.id }),
  component: CustomerDetail,
});

function CustomerDetail() {
  const customer = Route.useLoaderData();
  const router = useRouter();

  if (!customer) return <CustomerNotFound />;

  return <CustomerDetailPanel customer={customer} onArchiveSuccess={() => router.invalidate()} />;
}
