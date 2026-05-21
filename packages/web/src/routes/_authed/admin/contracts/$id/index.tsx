import { createFileRoute, getRouteApi, useRouter } from '@tanstack/react-router';
import { ContractDetailPanel } from '../-components/contract-detail-panel';

const parentRoute = getRouteApi('/_authed/admin/contracts/$id');

export const Route = createFileRoute('/_authed/admin/contracts/$id/')({
  component: ContractDetail,
});

function ContractDetail() {
  const { id } = Route.useParams();
  const { items } = parentRoute.useLoaderData();
  const router = useRouter();
  const contract = items.find((candidate) => candidate.id === id);

  if (!contract) {
    return (
      <div className="flex h-full items-center justify-center text-muted-foreground">
        <p>Contract not found.</p>
      </div>
    );
  }

  return <ContractDetailPanel contract={contract} onArchiveSuccess={() => router.invalidate()} />;
}
