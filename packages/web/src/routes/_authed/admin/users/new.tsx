import { createFileRoute } from '@tanstack/react-router';
import { UserForm } from './-components/form';
import { createUserFn } from '#/features/users/users.functions';

export const Route = createFileRoute('/_authed/admin/users/new')({
  component: NewUser,
});

function NewUser() {
  return <UserForm title="New User" initial={{}} onSubmit={(values) => createUserFn({ data: values })} />;
}
