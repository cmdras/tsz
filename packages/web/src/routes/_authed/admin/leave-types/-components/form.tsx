import { useForm } from '@tanstack/react-form';
import { useRouter } from '@tanstack/react-router';
import { toast } from 'sonner';
import { leaveTypeSchema, allowanceModes, type LeaveTypeInput } from '#/features/leave-types/leave-types.schemas';
import { FormFooter } from '#/components/form-footer';
import { Card, CardContent, CardHeader, CardTitle } from '#/components/ui/card';
import { Input } from '#/components/ui/input';
import { Label } from '#/components/ui/label';
import { TextField } from '#/components/text-field';
import { FieldError } from '#/components/field-error';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '#/components/ui/select';

interface LeaveTypeFormProps {
  initial: Partial<LeaveTypeInput>;
  onSubmit: (values: LeaveTypeInput) => Promise<unknown>;
  title: string;
  onDone?: () => void;
}

export function LeaveTypeForm({ initial, onSubmit, title, onDone }: LeaveTypeFormProps) {
  const router = useRouter();

  const navigateOnDone = () => {
    if (onDone) {
      onDone();
      return;
    }
    router.navigate({ to: '/admin/leave-types' });
  };

  const form = useForm({
    defaultValues: {
      name: initial.name ?? '',
      defaultDays: initial.defaultDays ?? 0,
      defaultMode: initial.defaultMode ?? 'Limited',
    } satisfies LeaveTypeInput,
    validators: {
      onChange: leaveTypeSchema,
    },
    onSubmit: async ({ value }) => {
      try {
        await onSubmit(value);
        toast.success('Leave type saved');
        navigateOnDone();
      } catch (error) {
        toast.error(error instanceof Error ? error.message : 'Failed to save leave type');
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

          <form.Field name="defaultMode">
            {(field) => (
              <div className="grid gap-1.5">
                <Label htmlFor={field.name}>Default Mode</Label>
                <Select
                  value={field.state.value}
                  onValueChange={(value) => field.handleChange(value as LeaveTypeInput['defaultMode'])}
                >
                  <SelectTrigger id={field.name} onBlur={field.handleBlur}>
                    <SelectValue placeholder="Select a mode" />
                  </SelectTrigger>
                  <SelectContent>
                    {allowanceModes.map((mode) => (
                      <SelectItem key={mode} value={mode}>
                        {mode}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <FieldError field={field} />
              </div>
            )}
          </form.Field>

          <form.Field name="defaultDays">
            {(field) => (
              <div className="grid gap-2">
                <Label htmlFor={field.name}>Default Days</Label>
                <Input
                  id={field.name}
                  type="number"
                  step="0.1"
                  min="0"
                  max="365"
                  value={String(field.state.value)}
                  onChange={(changeEvent) => field.handleChange(parseFloat(changeEvent.target.value) || 0)}
                  onBlur={field.handleBlur}
                />
                <FieldError field={field} />
              </div>
            )}
          </form.Field>

          <form.Subscribe selector={(state) => [state.canSubmit, state.isSubmitting] as const}>
            {([canSubmit, isSubmitting]) => (
              <FormFooter canSubmit={canSubmit} isPending={isSubmitting} onCancel={navigateOnDone} />
            )}
          </form.Subscribe>
        </form>
      </CardContent>
    </Card>
  );
}
