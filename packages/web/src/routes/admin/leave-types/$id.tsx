import { createFileRoute } from '@tanstack/react-router';
import { LeaveTypeForm } from './-components/form';
import { fetchLeaveTypeById, updateLeaveTypeFn } from '#/features/leave-types/leave-types.functions';
import { Alert, AlertDescription, AlertTitle } from '#/components/ui/alert';

export const Route = createFileRoute('/admin/leave-types/$id')({
  loader: ({ params }) => fetchLeaveTypeById({ data: params.id }),
  component: EditLeaveType,
});

function EditLeaveType() {
  const leaveType = Route.useLoaderData();
  const { id } = Route.useParams();

  if (!leaveType) {
    return (
      <Alert variant="destructive">
        <AlertTitle>Leave type not found</AlertTitle>
        <AlertDescription>No leave type exists with this ID.</AlertDescription>
      </Alert>
    );
  }

  return (
    <LeaveTypeForm
      title={`Edit Leave Type — ${leaveType.name}`}
      initial={{
        name: leaveType.name,
        defaultDays: leaveType.defaultDays,
      }}
      onSubmit={(values) => updateLeaveTypeFn({ data: { id, data: values } })}
    />
  );
}
