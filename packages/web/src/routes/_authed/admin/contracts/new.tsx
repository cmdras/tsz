import { createFileRoute } from '@tanstack/react-router';
import { ContractForm } from './-components/form';
import { fetchContractFormOptions, createContractFn } from '#/features/contracts/contracts.functions';

export const Route = createFileRoute('/_authed/admin/contracts/new')({
  loader: () => fetchContractFormOptions(),
  component: NewContract,
});

function NewContract() {
  const { customers, consultants } = Route.useLoaderData();

  return (
    <ContractForm
      title="New Contract"
      initial={{}}
      customers={customers}
      consultants={consultants}
      onSubmit={(values) => createContractFn({ data: values })}
    />
  );
}
