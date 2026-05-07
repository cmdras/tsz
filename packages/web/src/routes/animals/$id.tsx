import { createFileRoute, useRouter } from '@tanstack/react-router';
import { useState } from 'react';
import { fetchAnimalById, saveAnimal } from '#/api/animals/server';

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
      <main>
        <h1 className="text-2xl font-bold">Animal not found</h1>
        <p className="mt-2 text-gray-600">No animal exists with this ID.</p>
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
      <div className="flex items-center gap-4">
        <h1 className="text-2xl font-bold">
          {editing
            ? <input
                className="border-b border-gray-400 bg-transparent outline-none"
                value={form.name}
                onChange={e => setForm(f => ({ ...f, name: e.target.value }))}
                onKeyDown={handleKeyDown}
                autoFocus
              />
            : animal.name}
        </h1>
        {!editing && (
          <button onClick={startEditing} className="text-sm text-blue-600 hover:underline">
            Edit
          </button>
        )}
      </div>
      <dl className="mt-4 space-y-2 text-sm">
        <div className="flex gap-2">
          <dt className="font-semibold">Species:</dt>
          <dd>
            {editing
              ? <input
                  className="border-b border-gray-400 bg-transparent outline-none"
                  value={form.species}
                  onChange={e => setForm(f => ({ ...f, species: e.target.value }))}
                  onKeyDown={handleKeyDown}
                />
              : animal.species}
          </dd>
        </div>
        <div className="flex gap-2">
          <dt className="font-semibold">Age:</dt>
          <dd>
            {editing
              ? <input
                  type="number"
                  className="w-16 border-b border-gray-400 bg-transparent outline-none"
                  value={form.age}
                  onChange={e => setForm(f => ({ ...f, age: Number(e.target.value) }))}
                  onKeyDown={handleKeyDown}
                />
              : animal.age}
          </dd>
        </div>
      </dl>
      {editing && (
        <div className="mt-4 flex gap-3 text-sm">
          <button onClick={save} className="text-blue-600 hover:underline">Save</button>
          <button onClick={() => setEditing(false)} className="text-gray-500 hover:underline">Cancel</button>
        </div>
      )}
    </main>
  );
}
