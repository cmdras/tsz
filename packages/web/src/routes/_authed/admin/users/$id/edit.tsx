import { createFileRoute } from '@tanstack/react-router';
import { UserForm } from '../-components/form';
import { UserNotFound } from '../-components/user-not-found';
import { fetchUserById, listLeaveTypesForPickerFn, updateUserFn } from '#/features/users/users.functions';

export const Route = createFileRoute('/_authed/admin/users/$id/edit')({
  loader: async ({ params }) => {
    const [user, leaveTypes] = await Promise.all([fetchUserById({ data: params.id }), listLeaveTypesForPickerFn()]);
    return { user, leaveTypes };
  },
  component: EditUser,
});

function EditUser() {
  const { user, leaveTypes } = Route.useLoaderData();
  const { id } = Route.useParams();

  if (!user) return <UserNotFound />;

  return (
    <div className="p-6">
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
    </div>
  );
}
