import { useForm } from '@tanstack/react-form';
import { useRouter } from '@tanstack/react-router';
import { toast } from 'sonner';
import { customerSchema, type CustomerInput } from '#/features/customers/customers.schemas';
import { FormCard } from '#/components/form-card';
import { TextField } from '#/components/text-field';

interface CustomerFormProps {
  initial: Partial<CustomerInput>;
  onSubmit: (values: CustomerInput) => Promise<unknown>;
  title: string;
  onDone?: () => void;
}

export function CustomerForm({ initial, onSubmit, title, onDone }: CustomerFormProps) {
  const router = useRouter();
  const navigateOnDone = () => {
    if (onDone) {
      onDone();
      return;
    }
    router.navigate({ to: '/admin/customers' });
  };

  const form = useForm({
    defaultValues: {
      name: initial.name ?? '',
      street: initial.street ?? '',
      zip: initial.zip ?? '',
      city: initial.city ?? '',
      country: initial.country ?? 'Belgium',
      contactName: initial.contactName ?? '',
      contactEmail: initial.contactEmail ?? '',
    } satisfies CustomerInput,
    validators: {
      onChange: customerSchema,
    },
    onSubmit: async ({ value }) => {
      try {
        await onSubmit(value);
        toast.success('Customer saved');
        void router.invalidate();
        navigateOnDone();
      } catch {
        toast.error('Failed to save customer');
      }
    },
  });

  return (
    <FormCard title={title} form={form} onCancel={navigateOnDone}>
      <form.Field name="name">{(field) => <TextField field={field} label="Name" autoFocus />}</form.Field>
      <form.Field name="street">{(field) => <TextField field={field} label="Street" />}</form.Field>

      <div className="grid grid-cols-2 gap-4">
        <form.Field name="zip">{(field) => <TextField field={field} label="Zip" />}</form.Field>
        <form.Field name="city">{(field) => <TextField field={field} label="City" />}</form.Field>
      </div>

      <form.Field name="country">{(field) => <TextField field={field} label="Country" />}</form.Field>
      <form.Field name="contactName">{(field) => <TextField field={field} label="Contact Name" />}</form.Field>
      <form.Field name="contactEmail">
        {(field) => <TextField field={field} label="Contact Email" type="email" />}
      </form.Field>
    </FormCard>
  );
}
