import { useForm } from '@tanstack/react-form';
import type { Animal } from '#/api/animals';
import { updateAnimalSchema, type UpdateAnimalInput } from '../-schemas';
import { Button } from '#/components/ui/button';
import { Input } from '#/components/ui/input';
import { Label } from '#/components/ui/label';
import { FieldError } from '#/components/field-error';

interface AnimalFormProps {
  animal?: Animal | null;
  onSubmit: (values: UpdateAnimalInput) => Promise<void> | void;
  onCancel?: () => void;
  submitLabel?: string;
}

export function AnimalForm({ animal, onSubmit, onCancel, submitLabel = 'Save' }: AnimalFormProps) {
  const form = useForm({
    defaultValues: {
      name: animal?.name ?? '',
      species: animal?.species ?? '',
      age: animal?.age ?? 0,
    },
    validators: {
      onChange: updateAnimalSchema,
    },
    onSubmit: async ({ value }) => {
      await onSubmit(value);
    },
  });

  return (
    <form
      onSubmit={(submitEvent) => {
        submitEvent.preventDefault();
        submitEvent.stopPropagation();
        form.handleSubmit();
      }}
      className="grid gap-4"
    >
      <form.Field name="name">
        {(field) => (
          <div className="grid gap-2">
            <Label htmlFor={field.name}>Name</Label>
            <Input
              id={field.name}
              name={field.name}
              value={field.state.value}
              onBlur={field.handleBlur}
              onChange={(changeEvent) => field.handleChange(changeEvent.target.value)}
              autoFocus
            />
            <FieldError field={field} />
          </div>
        )}
      </form.Field>

      <form.Field name="species">
        {(field) => (
          <div className="grid gap-2">
            <Label htmlFor={field.name}>Species</Label>
            <Input
              id={field.name}
              name={field.name}
              value={field.state.value}
              onBlur={field.handleBlur}
              onChange={(changeEvent) => field.handleChange(changeEvent.target.value)}
            />
            <FieldError field={field} />
          </div>
        )}
      </form.Field>

      <form.Field name="age">
        {(field) => (
          <div className="grid gap-2">
            <Label htmlFor={field.name}>Age</Label>
            <Input
              id={field.name}
              name={field.name}
              type="number"
              value={field.state.value}
              onBlur={field.handleBlur}
              onChange={(changeEvent) =>
                field.handleChange(changeEvent.target.value === '' ? 0 : changeEvent.target.valueAsNumber)
              }
            />
            <FieldError field={field} />
          </div>
        )}
      </form.Field>

      <form.Subscribe selector={(state) => [state.canSubmit, state.isSubmitting] as const}>
        {([canSubmit, isSubmitting]) => (
          <div className="flex gap-2">
            <Button type="submit" size="sm" disabled={!canSubmit}>
              {isSubmitting ? 'Saving…' : submitLabel}
            </Button>
            {onCancel && (
              <Button type="button" size="sm" variant="outline" onClick={onCancel} disabled={isSubmitting}>
                Cancel
              </Button>
            )}
          </div>
        )}
      </form.Subscribe>
    </form>
  );
}
