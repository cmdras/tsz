import { createFileRoute, useNavigate } from '@tanstack/react-router';
import { fetchLeaveTypeById, updateLeaveTypeFn } from '#/features/leave-types/leave-types.functions';
import { LeaveTypeForm } from '../-components/form';
import { Alert, AlertDescription, AlertTitle } from '#/components/ui/alert';

export const Route = createFileRoute('/_authed/admin/leave-types/$id/edit')({
  loader: ({ params }) => fetchLeaveTypeById({ data: params.id }),
  component: EditLeaveType,
});

function EditLeaveType() {
  const leaveType = Route.useLoaderData();
  const { id } = Route.useParams();
  const navigate = useNavigate();

  if (!leaveType) {
    return (
      <Alert variant="destructive">
        <AlertTitle>Leave type not found</AlertTitle>
        <AlertDescription>No leave type exists with this ID.</AlertDescription>
      </Alert>
    );
  }

  return (
    <div className="p-6">
      <LeaveTypeForm
        title={`Edit Leave Type — ${leaveType.name}`}
        initial={{
          name: leaveType.name,
          defaultDays: leaveType.defaultDays,
          defaultMode: leaveType.defaultMode,
        }}
        onSubmit={(values) => updateLeaveTypeFn({ data: { id, data: values } })}
        onDone={() => navigate({ to: '/admin/leave-types/$id', params: { id } })}
      />
    </div>
  );
}
