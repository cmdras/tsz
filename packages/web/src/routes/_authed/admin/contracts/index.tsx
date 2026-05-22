import { createFileRoute } from '@tanstack/react-router';
import { fetchContracts } from '#/features/contracts/contracts.functions';
import { contractSearchSchema } from '#/features/contracts/contracts.schemas';
import { ContractEmptyPanel } from './-components/contract-empty-panel';
import { ContractsPageLayout } from './-components/contracts-page-layout';

export const Route = createFileRoute('/_authed/admin/contracts/')({
  validateSearch: contractSearchSchema,
  loaderDeps: ({ search }) => ({
    search: search.search,
    page: search.page,
    archived: search.archived,
  }),
  loader: ({ deps }) => fetchContracts({ data: deps }),
  staleTime: 30_000,
  component: ContractList,
});

function ContractList() {
  const { items, total } = Route.useLoaderData();
  const { search, page, archived } = Route.useSearch();

  return (
    <ContractsPageLayout contracts={items} total={total} search={search} page={page} archived={archived}>
      <ContractEmptyPanel />
    </ContractsPageLayout>
  );
}
