import { createFileRoute, useRouter } from '@tanstack/react-router';
import { useState } from 'react';
import { fetchAnimalById, saveAnimal } from '#/api/animals/server';

export const Route = createFileRoute('/animals/$id')({
  loader: ({ params }) => fetchAnimalById({ data: Number(params.id) }),
  component: AnimalDetail,
});

const inputClass =
  'w-full rounded-md border border-gray-300 bg-white px-3 py-1.5 text-sm shadow-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500';

function AnimalDetail() {
  const animal = Route.useLoaderData();
  const router = useRouter();
  const [editing, setEditing] = useState(false);
  const [form, setForm] = useState({ name: '', species: '', age: 0 });

  if (!animal) {
    return (
      <main className="rounded-lg border border-red-200 bg-red-50 p-6">
        <h1 className="text-xl font-semibold text-red-700">Animal not found</h1>
        <p className="mt-1 text-sm text-red-500">No animal exists with this ID.</p>
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
        <h1 className="text-2xl font-bold text-gray-900">
          {editing
            ? <input
                className={inputClass}
                value={form.name}
                onChange={e => setForm(f => ({ ...f, name: e.target.value }))}
                onKeyDown={handleKeyDown}
                autoFocus
              />
            : animal.name}
        </h1>
        {!editing && (
          <button
            onClick={startEditing}
            className="rounded-md bg-blue-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2"
          >
            Edit
          </button>
        )}
      </div>

      <dl className="mt-6 divide-y divide-gray-100 rounded-lg border border-gray-200 bg-white shadow-sm">
        <div className="flex items-center gap-4 px-4 py-3">
          <dt className="w-24 shrink-0 text-sm font-medium text-gray-500">Species</dt>
          <dd className="flex-1 text-sm text-gray-900">
            {editing
              ? <input
                  className={inputClass}
                  value={form.species}
                  onChange={e => setForm(f => ({ ...f, species: e.target.value }))}
                  onKeyDown={handleKeyDown}
                />
              : animal.species}
          </dd>
        </div>
        <div className="flex items-center gap-4 px-4 py-3">
          <dt className="w-24 shrink-0 text-sm font-medium text-gray-500">Age</dt>
          <dd className="flex-1 text-sm text-gray-900">
            {editing
              ? <input
                  type="number"
                  className={inputClass}
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
          <button
            onClick={save}
            className="rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2"
          >
            Save
          </button>
          <button
            onClick={() => setEditing(false)}
            className="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-gray-300 focus:ring-offset-2"
          >
            Cancel
          </button>
        </div>
      )}
    </main>
  );
}
