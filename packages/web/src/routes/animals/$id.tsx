import { createFileRoute, useRouter } from '@tanstack/react-router';
import { useState } from 'react';
import { useForm } from '@tanstack/react-form';
import { fetchAnimalById, saveAnimal } from '#/api/animals/server';
import { updateAnimalSchema } from '#/api/animals/schemas';
import { Button } from '#/components/ui/button';
import { Input } from '#/components/ui/input';
import { Label } from '#/components/ui/label';
import { FieldError } from '#/components/field-error';

export const Route = createFileRoute('/animals/$id')({
  loader: ({ params }) => fetchAnimalById({ data: Number(params.id) }),
  component: AnimalDetail,
});

function AnimalDetail() {
  const animal = Route.useLoaderData();
  const router = useRouter();
  const [editing, setEditing] = useState(false);

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
      await saveAnimal({ data: { id: animal!.id!, animal: value } });
      setEditing(false);
      router.invalidate();
    },
  });

  if (!animal) {
    return (
      <main className="rounded-lg border border-destructive/30 bg-destructive/10 p-6">
        <h1 className="text-xl font-semibold text-destructive">Animal not found</h1>
        <p className="mt-1 text-sm text-destructive/80">No animal exists with this ID.</p>
      </main>
    );
  }

  const startEditing = () => {
    form.setFieldValue('name', animal.name ?? '');
    form.setFieldValue('species', animal.species ?? '');
    form.setFieldValue('age', animal.age ?? 0);
    setEditing(true);
  };

  return (
    <form
      onSubmit={(e) => {
        e.preventDefault();
        form.handleSubmit();
      }}
    >
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-foreground">{animal.name}</h1>
        {!editing && (
          <Button type="button" size="sm" onClick={startEditing}>
            Edit
          </Button>
        )}
      </div>

      <dl className="mt-6 divide-y divide-border rounded-lg border border-border bg-card shadow-sm">
        <DetailRow label="Name">
          {editing ? (
            <form.Field name="name">
              {(field) => (
                <div className="grid gap-1">
                  <Label htmlFor={field.name} className="sr-only">
                    Name
                  </Label>
                  <Input
                    id={field.name}
                    name={field.name}
                    value={field.state.value}
                    onBlur={field.handleBlur}
                    onChange={(e) => field.handleChange(e.target.value)}
                    autoFocus
                  />
                  <FieldError field={field} />
                </div>
              )}
            </form.Field>
          ) : (
            animal.name
          )}
        </DetailRow>

        <DetailRow label="Species">
          {editing ? (
            <form.Field name="species">
              {(field) => (
                <div className="grid gap-1">
                  <Label htmlFor={field.name} className="sr-only">
                    Species
                  </Label>
                  <Input
                    id={field.name}
                    name={field.name}
                    value={field.state.value}
                    onBlur={field.handleBlur}
                    onChange={(e) => field.handleChange(e.target.value)}
                  />
                  <FieldError field={field} />
                </div>
              )}
            </form.Field>
          ) : (
            animal.species
          )}
        </DetailRow>

        <DetailRow label="Age">
          {editing ? (
            <form.Field name="age">
              {(field) => (
                <div className="grid gap-1">
                  <Label htmlFor={field.name} className="sr-only">
                    Age
                  </Label>
                  <Input
                    id={field.name}
                    name={field.name}
                    type="number"
                    value={field.state.value}
                    onBlur={field.handleBlur}
                    onChange={(e) => field.handleChange(e.target.valueAsNumber)}
                  />
                  <FieldError field={field} />
                </div>
              )}
            </form.Field>
          ) : (
            animal.age
          )}
        </DetailRow>
      </dl>

      {editing && (
        <div className="mt-4 flex gap-2">
          <Button type="submit" size="sm">
            Save
          </Button>
          <Button type="button" size="sm" variant="outline" onClick={() => setEditing(false)}>
            Cancel
          </Button>
        </div>
      )}
    </form>
  );
}

function DetailRow({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div className="flex items-center gap-4 px-4 py-3">
      <dt className="w-24 shrink-0 text-sm font-medium text-muted-foreground">{label}</dt>
      <dd className="flex-1 text-sm text-foreground">{children}</dd>
    </div>
  );
}
