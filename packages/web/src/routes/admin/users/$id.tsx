import { createFileRoute } from '@tanstack/react-router';
import { UserForm } from './-components/form';
import { fetchUserById, updateUserFn } from '#/features/users/users.functions';
import { Alert, AlertDescription, AlertTitle } from '#/components/ui/alert';

export const Route = createFileRoute('/admin/users/$id')({
  loader: ({ params }) => fetchUserById({ data: params.id }),
  component: EditUser,
});

function EditUser() {
  const user = Route.useLoaderData();
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
      }}
      onSubmit={(values) => updateUserFn({ data: { id, data: values } })}
    />
  );
}
