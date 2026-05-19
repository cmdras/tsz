import { createFileRoute } from '@tanstack/react-router';
import { LeaveTypeForm } from './-components/form';
import { createLeaveTypeFn } from '#/features/leave-types/leave-types.functions';

export const Route = createFileRoute('/_authed/admin/leave-types/new')({
  component: NewLeaveType,
});

function NewLeaveType() {
  return (
    <LeaveTypeForm title="New Leave Type" initial={{}} onSubmit={(values) => createLeaveTypeFn({ data: values })} />
  );
}
