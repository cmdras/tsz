import { createFileRoute } from '@tanstack/react-router';
import { getAnimalById } from '#/api/animals';

export const Route = createFileRoute('/animals_/$id')({
  loader: async ({ params }) => {
    try {
      const { data } = await getAnimalById(Number(params.id));
      return data ?? null;
    } catch (error) {
      console.log(error);
      throw error;
    }
  },
  component: AnimalDetail,
});

function AnimalDetail() {
  const animal = Route.useLoaderData();

  if (!animal) {
    return (
      <main>
        <h1 className="text-2xl font-bold">Animal not found</h1>
        <p className="mt-2 text-gray-600">No animal exists with this ID.</p>
      </main>
    );
  }

  return (
    <main>
      <h1 className="text-2xl font-bold">{animal.name}</h1>
      <dl className="mt-4 space-y-2 text-sm">
        <div className="flex gap-2">
          <dt className="font-semibold">Species:</dt>
          <dd>{animal.species}</dd>
        </div>
        <div className="flex gap-2">
          <dt className="font-semibold">Age:</dt>
          <dd>{animal.age}</dd>
        </div>
      </dl>
    </main>
  );
}
