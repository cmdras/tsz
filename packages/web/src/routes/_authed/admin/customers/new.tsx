import { createFileRoute } from '@tanstack/react-router';
import { CustomerForm } from './-components/form';
import { createCustomerFn } from '#/features/customers/customers.functions';

export const Route = createFileRoute('/_authed/admin/customers/new')({
  component: NewCustomer,
});

function NewCustomer() {
  return <CustomerForm title="New Customer" initial={{}} onSubmit={(values) => createCustomerFn({ data: values })} />;
}
