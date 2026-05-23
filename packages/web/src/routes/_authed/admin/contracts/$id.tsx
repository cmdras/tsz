import { createFileRoute, Outlet } from '@tanstack/react-router';
import { fetchContracts } from '#/features/contracts/contracts.functions';
import { contractSearchSchema } from '#/features/contracts/contracts.schemas';
import { ContractsPageLayout } from './-components/contracts-page-layout';

export const Route = createFileRoute('/_authed/admin/contracts/$id')({
  validateSearch: contractSearchSchema,
  loaderDeps: ({ search }) => ({
    search: search.search,
    page: search.page,
    filter: search.filter,
  }),
  loader: ({ deps }) => fetchContracts({ data: deps }),
  staleTime: 30_000,
  component: ContractDetailLayout,
});

function ContractDetailLayout() {
  const { items, total } = Route.useLoaderData();
  const { id } = Route.useParams();
  const { search, page, filter } = Route.useSearch();

  return (
    <ContractsPageLayout contracts={items} total={total} selectedId={id} search={search} page={page} filter={filter}>
      <Outlet />
    </ContractsPageLayout>
  );
}
