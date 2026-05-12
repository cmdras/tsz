import { createFileRoute } from '@tanstack/react-router';
import { CustomerForm } from './-components/customer-form';
import { fetchCustomerById, updateCustomerFn } from './-server';
import { Alert, AlertDescription, AlertTitle } from '#/components/ui/alert';

export const Route = createFileRoute('/admin/customers/$id')({
  loader: ({ params }) => fetchCustomerById({ data: params.id }),
  component: EditCustomer,
});

function EditCustomer() {
  const customer = Route.useLoaderData();
  const { id } = Route.useParams();

  if (!customer) {
    return (
      <Alert variant="destructive">
        <AlertTitle>Customer not found</AlertTitle>
        <AlertDescription>No customer exists with this ID.</AlertDescription>
      </Alert>
    );
  }

  return (
    <CustomerForm
      title={`Edit Customer #${String(customer.number).padStart(6, '0')}`}
      initial={{
        name: customer.name,
        street: customer.street,
        zip: customer.zip,
        city: customer.city,
        country: customer.country,
        contactName: customer.contactName,
        contactEmail: customer.contactEmail,
      }}
      onSubmit={(values) => updateCustomerFn({ data: { id, data: values } })}
    />
  );
}
