import { createFileRoute } from '@tanstack/react-router';
import { CustomerForm } from '../-components/form';
import { CustomerNotFound } from '../-components/customer-not-found';
import { fetchCustomerById, updateCustomerFn } from '#/features/customers/customers.functions';
import { formatEntityNumber } from '#/lib/utils';

export const Route = createFileRoute('/_authed/admin/customers/$id/edit')({
  loader: ({ params }) => fetchCustomerById({ data: params.id }),
  component: EditCustomer,
});

function EditCustomer() {
  const customer = Route.useLoaderData();
  const { id } = Route.useParams();

  if (!customer) return <CustomerNotFound />;

  return (
    <div className="p-6">
      <CustomerForm
        title={`Edit Customer #${formatEntityNumber(customer.number)}`}
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
    </div>
  );
}
