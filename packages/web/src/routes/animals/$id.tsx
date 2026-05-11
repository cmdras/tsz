import { createFileRoute, useRouter } from '@tanstack/react-router';
import { useState } from 'react';
import { fetchAnimalById, saveAnimal } from '#/api/animals/server';
import { Button } from '#/components/ui/button';
import { AnimalForm } from './-components/animal-form';

export const Route = createFileRoute('/animals/$id')({
  loader: ({ params }) => fetchAnimalById({ data: Number(params.id) }),
  component: AnimalDetail,
});

function AnimalDetail() {
  const animal = Route.useLoaderData();
  const router = useRouter();
  const [editing, setEditing] = useState(false);

  if (!animal) {
    return (
      <main className="rounded-lg border border-destructive/30 bg-destructive/10 p-6">
        <h1 className="text-xl font-semibold text-destructive">Animal not found</h1>
        <p className="mt-1 text-sm text-destructive/80">No animal exists with this ID.</p>
      </main>
    );
  }

  return (
    <main>
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-foreground">{animal.name}</h1>
        {!editing && (
          <Button type="button" size="sm" onClick={() => setEditing(true)}>
            Edit
          </Button>
        )}
      </div>

      {editing ? (
        <div className="mt-6">
          <AnimalForm
            animal={animal}
            onSubmit={async (values) => {
              await saveAnimal({ data: { id: animal.id!, animal: values } });
              setEditing(false);
              router.invalidate();
            }}
            onCancel={() => setEditing(false)}
          />
        </div>
      ) : (
        <dl className="mt-6 divide-y divide-border rounded-lg border border-border bg-card shadow-sm">
          <DetailRow label="Name">{animal.name}</DetailRow>
          <DetailRow label="Species">{animal.species}</DetailRow>
          <DetailRow label="Age">{animal.age}</DetailRow>
        </dl>
      )}
    </main>
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
