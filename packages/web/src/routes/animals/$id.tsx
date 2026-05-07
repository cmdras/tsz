import { createFileRoute, useRouter } from '@tanstack/react-router';
import { useState } from 'react';
import { useForm } from '@tanstack/react-form';
import { fetchAnimalById, saveAnimal } from '#/api/animals/server';
import { Button } from '#/components/ui/button';
import { Input } from '#/components/ui/input';

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
        <div className="flex items-center gap-4 px-4 py-3">
          <dt className="w-24 shrink-0 text-sm font-medium text-muted-foreground">Name</dt>
          <dd className="flex-1 text-sm text-foreground">
            {editing ? (
              <form.Field
                name="name"
                validators={{
                  onChange: ({ value }) => (!value.trim() ? 'Name is required' : undefined),
                }}
              >
                {(field) => (
                  <div>
                    <Input
                      value={field.state.value}
                      onChange={(e) => field.handleChange(e.target.value)}
                      onBlur={field.handleBlur}
                      autoFocus
                    />
                    {field.state.meta.errors.length > 0 && (
                      <p className="mt-1 text-xs text-destructive">{field.state.meta.errors.join(', ')}</p>
                    )}
                  </div>
                )}
              </form.Field>
            ) : (
              animal.name
            )}
          </dd>
        </div>

        <div className="flex items-center gap-4 px-4 py-3">
          <dt className="w-24 shrink-0 text-sm font-medium text-muted-foreground">Species</dt>
          <dd className="flex-1 text-sm text-foreground">
            {editing ? (
              <form.Field
                name="species"
                validators={{
                  onChange: ({ value }) => (!value.trim() ? 'Species is required' : undefined),
                }}
              >
                {(field) => (
                  <div>
                    <Input
                      value={field.state.value}
                      onChange={(e) => field.handleChange(e.target.value)}
                      onBlur={field.handleBlur}
                    />
                    {field.state.meta.errors.length > 0 && (
                      <p className="mt-1 text-xs text-destructive">{field.state.meta.errors.join(', ')}</p>
                    )}
                  </div>
                )}
              </form.Field>
            ) : (
              animal.species
            )}
          </dd>
        </div>

        <div className="flex items-center gap-4 px-4 py-3">
          <dt className="w-24 shrink-0 text-sm font-medium text-muted-foreground">Age</dt>
          <dd className="flex-1 text-sm text-foreground">
            {editing ? (
              <form.Field
                name="age"
                validators={{
                  onChange: ({ value }) => (value < 0 ? 'Age must be 0 or greater' : undefined),
                }}
              >
                {(field) => (
                  <div>
                    <Input
                      type="number"
                      value={field.state.value}
                      onChange={(e) => field.handleChange(e.target.valueAsNumber)}
                      onBlur={field.handleBlur}
                    />
                    {field.state.meta.errors.length > 0 && (
                      <p className="mt-1 text-xs text-destructive">{field.state.meta.errors.join(', ')}</p>
                    )}
                  </div>
                )}
              </form.Field>
            ) : (
              animal.age
            )}
          </dd>
        </div>
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
