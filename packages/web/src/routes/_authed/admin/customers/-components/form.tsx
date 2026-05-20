import { useForm } from '@tanstack/react-form';
import { useRouter } from '@tanstack/react-router';
import { toast } from 'sonner';
import { customerSchema, type CustomerInput } from '#/features/customers/customers.schemas';
import { Button } from '#/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '#/components/ui/card';
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
        navigateOnDone();
      } catch {
        toast.error('Failed to save customer');
      }
    },
  });

  return (
    <Card>
      <CardHeader>
        <CardTitle>{title}</CardTitle>
      </CardHeader>
      <CardContent>
        <form
          onSubmit={(submitEvent) => {
            submitEvent.preventDefault();
            submitEvent.stopPropagation();
            form.handleSubmit();
          }}
          className="grid gap-4"
        >
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

          <form.Subscribe selector={(state) => [state.canSubmit, state.isSubmitting] as const}>
            {([canSubmit, isSubmitting]) => (
              <div className="flex gap-2">
                <Button type="submit" disabled={!canSubmit}>
                  {isSubmitting ? 'Saving…' : 'Save'}
                </Button>
                <Button type="button" variant="outline" onClick={navigateOnDone} disabled={isSubmitting}>
                  Cancel
                </Button>
              </div>
            )}
          </form.Subscribe>
        </form>
      </CardContent>
    </Card>
  );
}
