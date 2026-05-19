import { createFileRoute } from '@tanstack/react-router';
import { ContractForm } from './-components/form';
import {
  fetchContractById,
  fetchContractFormOptions,
  updateContractFn,
} from '#/features/contracts/contracts.functions';
import { Alert, AlertDescription, AlertTitle } from '#/components/ui/alert';

export const Route = createFileRoute('/_authed/admin/contracts/$id')({
  loader: async ({ params }) => {
    const [contract, options] = await Promise.all([fetchContractById({ data: params.id }), fetchContractFormOptions()]);
    return { contract, ...options };
  },
  component: EditContract,
});

function EditContract() {
  const { contract, customers, consultants } = Route.useLoaderData();
  const { id } = Route.useParams();

  if (!contract) {
    return (
      <Alert variant="destructive">
        <AlertTitle>Contract not found</AlertTitle>
        <AlertDescription>No contract exists with this ID.</AlertDescription>
      </Alert>
    );
  }

  const activeTasks = contract.tasks.filter((task) => !task.isArchived);

  return (
    <ContractForm
      title={`Edit Contract #${String(contract.number).padStart(6, '0')}`}
      initial={{
        customerId: contract.customerId,
        consultantId: contract.consultantId,
        subject: contract.subject,
        startDate: contract.startDate,
        endDate: contract.endDate ?? '',
        tasks: activeTasks.map((task) => ({
          id: task.id,
          name: task.name,
          dayRate: task.dayRate,
        })),
      }}
      customers={customers}
      consultants={consultants}
      onSubmit={(values) => updateContractFn({ data: { id, data: values } })}
    />
  );
}
