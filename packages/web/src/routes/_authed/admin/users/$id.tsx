import { createFileRoute } from '@tanstack/react-router';
import { UserForm } from './-components/form';
import { fetchUserById, listLeaveTypesForPickerFn, updateUserFn } from '#/features/users/users.functions';
import { Alert, AlertDescription, AlertTitle } from '#/components/ui/alert';

export const Route = createFileRoute('/_authed/admin/users/$id')({
  loader: async ({ params }) => {
    const [user, leaveTypes] = await Promise.all([fetchUserById({ data: params.id }), listLeaveTypesForPickerFn()]);
    return { user, leaveTypes };
  },
  component: EditUser,
});

function EditUser() {
  const { user, leaveTypes } = Route.useLoaderData();
  const { id } = Route.useParams();

  if (!user) {
    return (
      <Alert variant="destructive">
        <AlertTitle>User not found</AlertTitle>
        <AlertDescription>No user exists with this ID.</AlertDescription>
      </Alert>
    );
  }

  return (
    <UserForm
      title="Edit User"
      initial={{
        name: user.name,
        email: user.email,
        role: user.role,
        leaves: user.leaves.map((leave) => ({
          id: leave.id,
          leaveTypeId: leave.leaveTypeId,
          mode: leave.mode,
          totalDays: leave.totalDays,
        })),
      }}
      leaveTypes={leaveTypes}
      leaveSummaries={user.leaves.map((leave) => ({
        leaveTypeId: leave.leaveTypeId,
        name: leave.name,
        taken: leave.taken,
        balance: leave.balance,
      }))}
      onSubmit={(values) => updateUserFn({ data: { id, data: values } })}
    />
  );
}
