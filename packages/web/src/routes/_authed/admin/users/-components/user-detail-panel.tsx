import { useState } from 'react';
import { Link } from '@tanstack/react-router';
import { toast } from 'sonner';
import { archiveUserFn } from '#/features/users/users.functions';
import type { User } from '#/features/users/users.server';
import { getAvatarColor } from '#/lib/utils';
import { roleLabels } from '#/features/users/users.schemas';
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

interface UserDetailPanelProps {
  user: User;
  onArchiveSuccess: () => void;
}

export function UserDetailPanel({ user, onArchiveSuccess }: UserDetailPanelProps) {
  const [isArchivePending, setIsArchivePending] = useState(false);

  const handleArchive = async () => {
    setIsArchivePending(true);
    try {
      await archiveUserFn({ data: user.id });
      toast.success('User archived');
      onArchiveSuccess();
    } catch {
      toast.error('Failed to archive user');
    } finally {
      setIsArchivePending(false);
    }
  };

  return (
    <div className="flex flex-col gap-6 p-6">
      <div className="flex items-start gap-4">
        <div
          className="w-[72px] h-[72px] rounded-full flex items-center justify-center text-2xl font-bold flex-shrink-0 text-white"
          style={{ backgroundColor: getAvatarColor(user.name) }}
        >
          {user.name.slice(0, 2).toUpperCase()}
        </div>
        <div className="flex-1">
          <h2 className="text-3xl font-bold leading-tight">{user.name}</h2>
          <div className="flex items-center gap-2 mt-1">
            {user.isArchived ? (
              <Badge variant="secondary">Archived</Badge>
            ) : (
              <Badge variant="outline" className="border-primary text-primary">
                Active
              </Badge>
            )}
            <Badge variant="outline">{roleLabels[user.role]}</Badge>
          </div>
        </div>
        <div className="flex gap-2 flex-shrink-0">
          <Button variant="outline" asChild>
            <Link to="/admin/users/$id/edit" params={{ id: user.id }}>
              Edit
            </Link>
          </Button>
          {!user.isArchived && (
            <AlertDialog>
              <AlertDialogTrigger asChild>
                <Button variant="outline" disabled={isArchivePending}>
                  Archive
                </Button>
              </AlertDialogTrigger>
              <AlertDialogContent>
                <AlertDialogHeader>
                  <AlertDialogTitle>Archive user?</AlertDialogTitle>
                  <AlertDialogDescription>
                    {user.name} will be removed from the active user list.
                  </AlertDialogDescription>
                </AlertDialogHeader>
                <AlertDialogFooter>
                  <AlertDialogCancel>Cancel</AlertDialogCancel>
                  <AlertDialogAction onClick={handleArchive}>Archive</AlertDialogAction>
                </AlertDialogFooter>
              </AlertDialogContent>
            </AlertDialog>
          )}
        </div>
      </div>

      <div className="grid grid-cols-3 gap-4">
        <FieldValue label="Name" value={user.name} className="col-span-2" />
        <FieldValue label="Email" value={user.email} className="col-span-2" />
        <FieldValue label="Role" value={roleLabels[user.role]} />
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
