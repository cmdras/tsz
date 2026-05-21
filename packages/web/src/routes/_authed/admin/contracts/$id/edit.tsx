import { createFileRoute, useNavigate } from '@tanstack/react-router';
import { ContractForm } from '../-components/form';
import {
  fetchContractById,
  fetchContractFormOptions,
  updateContractFn,
} from '#/features/contracts/contracts.functions';
import { Alert, AlertDescription, AlertTitle } from '#/components/ui/alert';
import { formatEntityNumber } from '#/lib/utils';

export const Route = createFileRoute('/_authed/admin/contracts/$id/edit')({
  loader: async ({ params }) => {
    const [contract, options] = await Promise.all([fetchContractById({ data: params.id }), fetchContractFormOptions()]);
    return { contract, ...options };
  },
  component: EditContract,
});

function EditContract() {
  const { contract, customers, consultants } = Route.useLoaderData();
  const { id } = Route.useParams();
  const navigate = useNavigate();

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
    <div className="p-6">
      <ContractForm
        title={`Edit Contract #${formatEntityNumber(contract.number)}`}
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
        onSubmit={(values) => updateContractFn({ data: { id: contract.id, data: values } })}
        onDone={() => navigate({ to: '/admin/contracts/$id', params: { id } })}
      />
    </div>
  );
}
