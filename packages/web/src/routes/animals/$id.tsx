import { createFileRoute, useRouter } from '@tanstack/react-router';
import { useState } from 'react';
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
  const [form, setForm] = useState({ name: '', species: '', age: 0 });

  if (!animal) {
    return (
      <main className="rounded-lg border border-destructive/30 bg-destructive/10 p-6">
        <h1 className="text-xl font-semibold text-destructive">Animal not found</h1>
        <p className="mt-1 text-sm text-destructive/80">No animal exists with this ID.</p>
      </main>
    );
  }

  const startEditing = () => {
    setForm({ name: animal.name ?? '', species: animal.species ?? '', age: animal.age ?? 0 });
    setEditing(true);
  };

  const save = async () => {
    await saveAnimal({ data: { id: animal.id!, animal: form } });
    setEditing(false);
    router.invalidate();
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') save();
  };

  return (
    <main>
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-foreground">{animal.name}</h1>
        {!editing && (
          <Button size="sm" onClick={startEditing}>Edit</Button>
        )}
      </div>

      <dl className="mt-6 divide-y divide-border rounded-lg border border-border bg-card shadow-sm">
        <div className="flex items-center gap-4 px-4 py-3">
          <dt className="w-24 shrink-0 text-sm font-medium text-muted-foreground">Name</dt>
          <dd className="flex-1 text-sm text-foreground">
            {editing
              ? <Input
                  value={form.name}
                  onChange={e => setForm(f => ({ ...f, name: e.target.value }))}
                  onKeyDown={handleKeyDown}
                  autoFocus
                />
              : animal.name}
          </dd>
        </div>
        <div className="flex items-center gap-4 px-4 py-3">
          <dt className="w-24 shrink-0 text-sm font-medium text-muted-foreground">Species</dt>
          <dd className="flex-1 text-sm text-foreground">
            {editing
              ? <Input
                  value={form.species}
                  onChange={e => setForm(f => ({ ...f, species: e.target.value }))}
                  onKeyDown={handleKeyDown}
                />
              : animal.species}
          </dd>
        </div>
        <div className="flex items-center gap-4 px-4 py-3">
          <dt className="w-24 shrink-0 text-sm font-medium text-muted-foreground">Age</dt>
          <dd className="flex-1 text-sm text-foreground">
            {editing
              ? <Input
                  type="number"
                  value={form.age}
                  onChange={e => setForm(f => ({ ...f, age: Number(e.target.value) }))}
                  onKeyDown={handleKeyDown}
                />
              : animal.age}
          </dd>
        </div>
      </dl>

      {editing && (
        <div className="mt-4 flex gap-2">
          <Button size="sm" onClick={save}>Save</Button>
          <Button size="sm" variant="outline" onClick={() => setEditing(false)}>Cancel</Button>
        </div>
      )}
    </main>
  );
}
