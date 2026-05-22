import { useState } from 'react';
import { Link } from '@tanstack/react-router';
import { toast } from 'sonner';
import { archiveLeaveTypeFn, unarchiveLeaveTypeFn } from '#/features/leave-types/leave-types.functions';
import type { LeaveType } from '#/features/leave-types/leave-types.server';
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

interface LeaveTypeDetailPanelProps {
  leaveType: LeaveType;
  onArchiveSuccess: () => void;
}

export function LeaveTypeDetailPanel({ leaveType, onArchiveSuccess }: LeaveTypeDetailPanelProps) {
  const [isActionPending, setIsActionPending] = useState(false);
  const archiveLabel = leaveType.isArchived ? 'Unarchive' : 'Archive';

  const handleToggleArchive = async () => {
    setIsActionPending(true);
    try {
      if (leaveType.isArchived) {
        await unarchiveLeaveTypeFn({ data: leaveType.id });
        toast.success('Leave type unarchived');
      } else {
        await archiveLeaveTypeFn({ data: leaveType.id });
        toast.success('Leave type archived');
      }
      onArchiveSuccess();
    } catch {
      toast.error(`Failed to ${archiveLabel.toLowerCase()} leave type`);
    } finally {
      setIsActionPending(false);
    }
  };

  return (
    <div className="flex flex-col gap-6 p-6">
      <div className="flex items-start justify-between gap-4">
        <div>
          <h2 className="text-3xl font-bold leading-tight">{leaveType.name}</h2>
          <div className="flex items-center gap-2 mt-2">
            {leaveType.isArchived ? (
              <Badge variant="secondary">Archived</Badge>
            ) : (
              <Badge variant="outline" className="border-primary text-primary">
                {leaveType.defaultMode}
              </Badge>
            )}
          </div>
        </div>
        <div className="flex gap-2 flex-shrink-0">
          <Button variant="outline" asChild>
            <Link to="/admin/leave-types/$id/edit" params={{ id: leaveType.id }} search={(previous) => previous}>
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
                <AlertDialogTitle>{archiveLabel} leave type?</AlertDialogTitle>
                <AlertDialogDescription>
                  {leaveType.isArchived
                    ? `${leaveType.name} will be restored to the active leave type list.`
                    : `${leaveType.name} will be removed from the active leave type list.`}
                </AlertDialogDescription>
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
        <FieldValue label="Default Days" value={String(leaveType.defaultDays)} />
        <FieldValue label="Default Mode" value={leaveType.defaultMode} />
      </div>
    </div>
  );
}
