import { createFileRoute } from '@tanstack/react-router';
import { CustomerForm } from './-components/customer-form';
import { createCustomerFn } from './-server';

export const Route = createFileRoute('/admin/customers/new')({
  component: NewCustomer,
});

function NewCustomer() {
  return <CustomerForm title="New Customer" initial={{}} onSubmit={(values) => createCustomerFn({ data: values })} />;
}
