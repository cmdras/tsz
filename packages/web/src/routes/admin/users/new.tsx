import { createFileRoute } from '@tanstack/react-router';
import { UserForm } from './-components/user-form';
import { createUserFn } from './-server';

export const Route = createFileRoute('/admin/users/new')({
  component: NewUser,
});

function NewUser() {
  return <UserForm title="New User" initial={{}} onSubmit={(values) => createUserFn({ data: values })} />;
}
