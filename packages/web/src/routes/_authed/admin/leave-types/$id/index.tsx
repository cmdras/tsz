import { createFileRoute, getRouteApi, useRouter } from '@tanstack/react-router';
import { LeaveTypeDetailPanel } from '../-components/leave-type-detail-panel';

const parentRoute = getRouteApi('/_authed/admin/leave-types/$id');

export const Route = createFileRoute('/_authed/admin/leave-types/$id/')({
  component: LeaveTypeDetail,
});

function LeaveTypeDetail() {
  const { id } = Route.useParams();
  const { items } = parentRoute.useLoaderData();
  const router = useRouter();
  const leaveType = items.find((candidate) => candidate.id === id);

  if (!leaveType) {
    return (
      <div className="flex h-full items-center justify-center text-muted-foreground">
        <p>Leave type not found.</p>
      </div>
    );
  }

  return <LeaveTypeDetailPanel leaveType={leaveType} onArchiveSuccess={() => router.invalidate()} />;
}
