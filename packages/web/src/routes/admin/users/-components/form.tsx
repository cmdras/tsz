import { useForm } from '@tanstack/react-form';
import { useRouter } from '@tanstack/react-router';
import { toast } from 'sonner';
import { userSchema, userRoles, roleLabels, type UserInput } from '#/features/users/users.schemas';
import { Button } from '#/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '#/components/ui/card';
import { Label } from '#/components/ui/label';
import { TextField } from '#/components/text-field';
import { FieldError } from '#/components/field-error';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '#/components/ui/select';

interface UserFormProps {
  initial: Partial<UserInput>;
  onSubmit: (values: UserInput) => Promise<unknown>;
  title: string;
}

export function UserForm({ initial, onSubmit, title }: UserFormProps) {
  const router = useRouter();

  const form = useForm({
    defaultValues: {
      name: initial.name ?? '',
      email: initial.email ?? '',
      role: initial.role ?? 'User',
    } satisfies UserInput,
    validators: {
      onChange: userSchema,
    },
    onSubmit: async ({ value }) => {
      try {
        await onSubmit(value);
        toast.success('User saved');
        router.navigate({ to: '/admin/users' });
      } catch (error) {
        if (error instanceof Error && error.message === 'EMAIL_ALREADY_IN_USE') {
          toast.error('Email address is already in use');
        } else {
          toast.error('Failed to save user');
        }
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
          <form.Field name="email">{(field) => <TextField field={field} label="Email" type="email" />}</form.Field>
          <form.Field name="role">
            {(field) => (
              <div className="grid gap-1.5">
                <Label htmlFor={field.name}>Role</Label>
                <Select
                  value={field.state.value}
                  onValueChange={(value) => field.handleChange(value as UserInput['role'])}
                >
                  <SelectTrigger id={field.name} onBlur={field.handleBlur}>
                    <SelectValue placeholder="Select a role" />
                  </SelectTrigger>
                  <SelectContent>
                    {userRoles.map((role) => (
                      <SelectItem key={role} value={role}>
                        {roleLabels[role]}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <FieldError field={field} />
              </div>
            )}
          </form.Field>

          <form.Subscribe selector={(state) => [state.canSubmit, state.isSubmitting] as const}>
            {([canSubmit, isSubmitting]) => (
              <div className="flex gap-2">
                <Button type="submit" disabled={!canSubmit}>
                  {isSubmitting ? 'Saving…' : 'Save'}
                </Button>
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => router.navigate({ to: '/admin/users' })}
                  disabled={isSubmitting}
                >
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
