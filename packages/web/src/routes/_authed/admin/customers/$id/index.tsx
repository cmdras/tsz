import { createFileRoute, getRouteApi, useRouter } from '@tanstack/react-router';
import { CustomerDetailPanel } from '../-components/customer-detail-panel';
import { CustomerNotFound } from '../-components/customer-not-found';

const parentRoute = getRouteApi('/_authed/admin/customers/$id');

export const Route = createFileRoute('/_authed/admin/customers/$id/')({
  component: CustomerDetail,
});

function CustomerDetail() {
  const { id } = Route.useParams();
  const { items } = parentRoute.useLoaderData();
  const router = useRouter();

  const customer = items.find((candidate) => candidate.id === id);

  if (!customer) return <CustomerNotFound />;

  return <CustomerDetailPanel customer={customer} onArchiveSuccess={() => router.invalidate()} />;
}
