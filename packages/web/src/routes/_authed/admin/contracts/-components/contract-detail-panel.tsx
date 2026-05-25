import { useState } from 'react';
import { Link } from '@tanstack/react-router';
import { toast } from 'sonner';
import { archiveContractFn, unarchiveContractFn } from '#/features/contracts/contracts.functions';
import type { Contract } from '#/features/contracts/contracts.server';
import { formatEntityNumber } from '#/lib/utils';
import { FieldValue } from '#/components/field-value';
import { Button } from '#/components/ui/button';
import { Badge } from '#/components/ui/badge';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from '#/components/ui/alert-dialog';

interface ContractDetailPanelProps {
  contract: Contract;
  onArchiveSuccess: () => void;
}

function archiveMessage(subject: string, isArchived: boolean) {
  return isArchived
    ? `${subject} will be restored to the active contract list.`
    : `${subject} will be removed from the active contract list.`;
}

function ContractStatusBadge({ contract }: { contract: Contract }) {
  if (contract.isArchived) {
    return <Badge variant="secondary">Archived</Badge>;
  }
  return (
    <Badge variant="outline" className="border-primary text-primary">
      {contract.endDate ? 'Active' : 'Open-ended'}
    </Badge>
  );
}

export function ContractDetailPanel({ contract, onArchiveSuccess }: ContractDetailPanelProps) {
  const [isActionPending, setIsActionPending] = useState(false);
  const archiveLabel = contract.isArchived ? 'Unarchive' : 'Archive';

  const handleToggleArchive = async () => {
    setIsActionPending(true);
    try {
      if (contract.isArchived) {
        await unarchiveContractFn({ data: contract.id });
        toast.success('Contract unarchived');
      } else {
        await archiveContractFn({ data: contract.id });
        toast.success('Contract archived');
      }
      onArchiveSuccess();
    } catch {
      toast.error(`Failed to ${archiveLabel.toLowerCase()} contract`);
    } finally {
      setIsActionPending(false);
    }
  };

  const activeTasks = contract.tasks.filter((task) => !task.isArchived);

  return (
    <div className="flex flex-col gap-6 p-6">
      <div className="flex items-start justify-between gap-4">
        <div>
          <p className="text-xs text-muted-foreground font-mono">
            Contract #{formatEntityNumber(contract.number)} · {contract.customerName}
          </p>
          <h2 className="text-3xl font-bold leading-tight mt-1">{contract.subject}</h2>
          <div className="flex items-center gap-2 mt-2">
            <ContractStatusBadge contract={contract} />
          </div>
        </div>
        <div className="flex gap-2 flex-shrink-0">
          <Button variant="outline" asChild>
            <Link to="/admin/contracts/$id/edit" params={{ id: contract.id }} search={(previous) => previous}>
              Edit
            </Link>
          </Button>
          <AlertDialog>
            <AlertDialogTrigger asChild>
              <Button variant="outline" disabled={isActionPending}>
                {archiveLabel}
              </Button>
            </AlertDialogTrigger>
            <AlertDialogContent>
              <AlertDialogHeader>
                <AlertDialogTitle>{archiveLabel} contract?</AlertDialogTitle>
                <AlertDialogDescription>{archiveMessage(contract.subject, contract.isArchived)}</AlertDialogDescription>
              </AlertDialogHeader>
              <AlertDialogFooter>
                <AlertDialogCancel>Cancel</AlertDialogCancel>
                <AlertDialogAction onClick={handleToggleArchive}>{archiveLabel}</AlertDialogAction>
              </AlertDialogFooter>
            </AlertDialogContent>
          </AlertDialog>
        </div>
      </div>

      <div className="grid grid-cols-2 gap-x-8 gap-y-4">
        <FieldValue label="Customer" value={contract.customerName} />
        <FieldValue label="Consultant" value={contract.consultantName} />
        <FieldValue label="Start date" value={contract.startDate} />
        <FieldValue label="End date" value={contract.endDate ?? '— (open)'} />
      </div>

      {activeTasks.length > 0 && (
        <div>
          <p className="text-[11px] font-medium uppercase tracking-[0.2em] text-muted-foreground mb-2">Tasks</p>
          <div className="divide-y border rounded-lg">
            {activeTasks.map((task) => (
              <div key={task.id} className="flex items-center justify-between px-3 py-2">
                <span className="text-sm">{task.name}</span>
                <span className="text-sm text-muted-foreground">€ {task.dayRate} / d</span>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
