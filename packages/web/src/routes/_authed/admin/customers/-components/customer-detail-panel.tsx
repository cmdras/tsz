import { useState } from 'react';
import { Link } from '@tanstack/react-router';
import { toast } from 'sonner';
import { archiveCustomerFn, unarchiveCustomerFn } from '#/features/customers/customers.functions';
import type { Customer } from '#/features/customers/customers.server';
import { formatEntityNumber, getAvatarColor } from '#/lib/utils';
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

interface CustomerDetailPanelProps {
  customer: Customer;
  onArchiveSuccess: () => void;
}

export function CustomerDetailPanel({ customer, onArchiveSuccess }: CustomerDetailPanelProps) {
  const [isActionPending, setIsActionPending] = useState(false);

  const formattedNumber = `#${formatEntityNumber(customer.number)}`;
  const archiveLabel = customer.isArchived ? 'Unarchive' : 'Archive';
  const archiveTitle = customer.isArchived ? 'Unarchive customer?' : 'Archive customer?';
  const archiveDescription = customer.isArchived
    ? `${customer.name} will be restored to the active customer list.`
    : `${customer.name} will be removed from the active customer list.`;

  const handleToggleArchive = async () => {
    setIsActionPending(true);
    try {
      if (customer.isArchived) {
        await unarchiveCustomerFn({ data: customer.id });
        toast.success('Customer unarchived');
      } else {
        await archiveCustomerFn({ data: customer.id });
        toast.success('Customer archived');
      }
      onArchiveSuccess();
    } catch {
      toast.error(`Failed to ${archiveLabel.toLowerCase()} customer`);
    } finally {
      setIsActionPending(false);
    }
  };

  return (
    <div className="flex flex-col gap-6 p-6">
      <div className="flex items-start gap-4">
        <div
          className="w-[72px] h-[72px] rounded-full flex items-center justify-center text-2xl font-bold flex-shrink-0 text-white"
          style={{ backgroundColor: getAvatarColor(customer.name) }}
        >
          {customer.name.slice(0, 2).toUpperCase()}
        </div>
        <div className="flex-1">
          <p className="text-xs text-muted-foreground font-mono">{formattedNumber}</p>
          <h2 className="text-3xl font-bold leading-tight">{customer.name}</h2>
          <div className="flex items-center gap-2 mt-1">
            {customer.isArchived ? (
              <Badge variant="secondary">Archived</Badge>
            ) : (
              <Badge variant="outline" className="border-primary text-primary">
                Active
              </Badge>
            )}
            {customer.city && <Badge variant="outline">{customer.city}</Badge>}
          </div>
        </div>
        <div className="flex gap-2 flex-shrink-0">
          <Button variant="outline" asChild>
            <Link to="/admin/customers/$id/edit" params={{ id: customer.id }}>
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
                <AlertDialogTitle>{archiveTitle}</AlertDialogTitle>
                <AlertDialogDescription>{archiveDescription}</AlertDialogDescription>
              </AlertDialogHeader>
              <AlertDialogFooter>
                <AlertDialogCancel>Cancel</AlertDialogCancel>
                <AlertDialogAction onClick={handleToggleArchive}>{archiveLabel}</AlertDialogAction>
              </AlertDialogFooter>
            </AlertDialogContent>
          </AlertDialog>
        </div>
      </div>

      <div className="grid grid-cols-3 gap-4">
        <FieldValue label="Name" value={customer.name} />
        <FieldValue label="Number" value={formattedNumber} />
        <div />
        <FieldValue label="Contact name" value={customer.contactName} />
        <FieldValue label="Email" value={customer.contactEmail} className="col-span-2" />
        <FieldValue label="Street" value={customer.street} className="col-span-2" />
        <div />
        <FieldValue label="Zip" value={customer.zip} />
        <FieldValue label="City" value={customer.city} />
        <FieldValue label="Country" value={customer.country} />
      </div>
    </div>
  );
}

function FieldValue({ label, value, className }: { label: string; value: string; className?: string }) {
  return (
    <div className={className}>
      <p className="text-[11px] font-medium uppercase tracking-[0.2em] text-muted-foreground">{label}</p>
      <p className="text-sm mt-0.5">{value || '—'}</p>
    </div>
  );
}
