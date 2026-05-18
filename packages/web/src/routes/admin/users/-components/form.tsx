import { useMemo } from 'react';
import { useForm } from '@tanstack/react-form';
import { useRouter } from '@tanstack/react-router';
import { toast } from 'sonner';
import {
  userSchema,
  userRoles,
  roleLabels,
  type UserInput,
  type UserLeaveAllowanceInput,
} from '#/features/users/users.schemas';
import { allowanceModes, type AllowanceMode } from '#/features/leave-types/leave-types.schemas';
import type { LeaveType } from '#/features/leave-types/leave-types.server';
import { Button } from '#/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '#/components/ui/card';
import { Input } from '#/components/ui/input';
import { Label } from '#/components/ui/label';
import { TextField } from '#/components/text-field';
import { FieldError } from '#/components/field-error';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '#/components/ui/select';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '#/components/ui/table';

interface LeaveSummary {
  leaveTypeId: string;
  name: string;
  taken: number;
  balance: number | null;
}

interface UserFormProps {
  initial: Partial<UserInput>;
  onSubmit: (values: UserInput) => Promise<unknown>;
  title: string;
  leaveTypes?: LeaveType[];
  leaveSummaries?: LeaveSummary[];
}

const PICKER_PLACEHOLDER = '__pick__';

export function UserForm({ initial, onSubmit, title, leaveTypes, leaveSummaries }: UserFormProps) {
  const router = useRouter();

  const form = useForm({
    defaultValues: {
      name: initial.name ?? '',
      email: initial.email ?? '',
      role: initial.role ?? 'User',
      leaves: initial.leaves ?? [],
    } satisfies UserInput,
    validators: {
      onChange: userSchema,
    },
    onSubmit: async ({ value }) => {
      try {
        await onSubmit(value);
        toast.success('User saved');
        router.navigate({ to: '/admin/users' });
      } catch (error) {
        if (error instanceof Error && error.message === 'EMAIL_ALREADY_IN_USE') {
          toast.error('Email address is already in use');
        } else if (error instanceof Error && error.message === 'DUPLICATE_LEAVE_ALLOWANCE') {
          toast.error('Duplicate leave allowance for this leave type');
        } else {
          toast.error('Failed to save user');
        }
      }
    },
  });

  const leaveTypeById = useMemo(
    () => new Map((leaveTypes ?? []).map((leaveType) => [leaveType.id, leaveType])),
    [leaveTypes],
  );
  const summaryByLeaveTypeId = useMemo(
    () => new Map((leaveSummaries ?? []).map((summary) => [summary.leaveTypeId, summary])),
    [leaveSummaries],
  );

  return (
    <form
      onSubmit={(submitEvent) => {
        submitEvent.preventDefault();
        submitEvent.stopPropagation();
        form.handleSubmit();
      }}
      className="grid gap-4"
    >
      <Card>
        <CardHeader>
          <CardTitle>{title}</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4">
            <form.Field name="name">{(field) => <TextField field={field} label="Name" autoFocus />}</form.Field>
            <form.Field name="email">{(field) => <TextField field={field} label="Email" type="email" />}</form.Field>
            <form.Field name="role">
              {(field) => (
                <div className="grid gap-1.5">
                  <Label htmlFor={field.name}>Role</Label>
                  <Select
                    value={field.state.value}
                    onValueChange={(value) => field.handleChange(value as UserInput['role'])}
                  >
                    <SelectTrigger id={field.name} onBlur={field.handleBlur}>
                      <SelectValue placeholder="Select a role" />
                    </SelectTrigger>
                    <SelectContent>
                      {userRoles.map((role) => (
                        <SelectItem key={role} value={role}>
                          {roleLabels[role]}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <FieldError field={field} />
                </div>
              )}
            </form.Field>
          </div>
        </CardContent>
      </Card>

      {leaveTypes && (
        <Card>
          <CardHeader>
            <CardTitle>Leaves</CardTitle>
          </CardHeader>
          <CardContent>
            <form.Field name="leaves" mode="array">
              {(leavesField) => {
                const usedLeaveTypeIds = new Set(leavesField.state.value.map((leave) => leave.leaveTypeId));
                const availableLeaveTypes = leaveTypes.filter((leaveType) => !usedLeaveTypeIds.has(leaveType.id));

                return (
                  <div className="grid gap-2">
                    <Table>
                      <TableHeader>
                        <TableRow>
                          <TableHead>Name</TableHead>
                          <TableHead className="w-40">Mode</TableHead>
                          <TableHead className="w-32">Total Days</TableHead>
                          <TableHead className="w-24">Taken</TableHead>
                          <TableHead className="w-24">Balance</TableHead>
                          <TableHead className="w-10" />
                        </TableRow>
                      </TableHeader>
                      <TableBody>
                        {leavesField.state.value.length === 0 && (
                          <TableRow>
                            <TableCell colSpan={6} className="text-center text-muted-foreground">
                              No leaves configured.
                            </TableCell>
                          </TableRow>
                        )}
                        {leavesField.state.value.map((leave, index) => {
                          const leaveType = leaveTypeById.get(leave.leaveTypeId);
                          const summary = summaryByLeaveTypeId.get(leave.leaveTypeId);
                          const name = leaveType?.name ?? summary?.name ?? '—';
                          const taken = summary?.taken ?? 0;
                          const balance = leave.mode === 'Limited' ? leave.totalDays - taken : null;
                          return (
                            <TableRow key={leave.id ?? `new-${leave.leaveTypeId}-${index}`}>
                              <TableCell className="py-1">{name}</TableCell>
                              <TableCell className="py-1">
                                <form.Field name={`leaves[${index}].mode`}>
                                  {(field) => (
                                    <Select
                                      value={field.state.value}
                                      onValueChange={(value) => {
                                        const nextMode = value as AllowanceMode;
                                        field.handleChange(nextMode);
                                        if (nextMode === 'Unlimited') {
                                          form.setFieldValue(`leaves[${index}].totalDays`, 0);
                                        }
                                      }}
                                    >
                                      <SelectTrigger id={field.name} onBlur={field.handleBlur}>
                                        <SelectValue placeholder="Mode" />
                                      </SelectTrigger>
                                      <SelectContent>
                                        {allowanceModes.map((mode) => (
                                          <SelectItem key={mode} value={mode}>
                                            {mode}
                                          </SelectItem>
                                        ))}
                                      </SelectContent>
                                    </Select>
                                  )}
                                </form.Field>
                              </TableCell>
                              <TableCell className="py-1">
                                <form.Field name={`leaves[${index}].totalDays`}>
                                  {(field) => (
                                    <>
                                      <Input
                                        id={field.name}
                                        type="number"
                                        step="any"
                                        min="0"
                                        max="365"
                                        disabled={leave.mode === 'Unlimited'}
                                        value={String(field.state.value)}
                                        onChange={(changeEvent) =>
                                          field.handleChange(parseFloat(changeEvent.target.value) || 0)
                                        }
                                        onBlur={field.handleBlur}
                                      />
                                      <FieldError field={field} />
                                    </>
                                  )}
                                </form.Field>
                              </TableCell>
                              <TableCell className="py-1 text-muted-foreground">{taken}</TableCell>
                              <TableCell className="py-1 text-muted-foreground">
                                {balance === null ? '—' : balance}
                              </TableCell>
                              <TableCell className="py-1">
                                <Button
                                  type="button"
                                  variant="ghost"
                                  size="sm"
                                  onClick={() => leavesField.removeValue(index)}
                                >
                                  ×
                                </Button>
                              </TableCell>
                            </TableRow>
                          );
                        })}
                      </TableBody>
                    </Table>

                    <div className="flex items-center gap-2">
                      <Select
                        value={PICKER_PLACEHOLDER}
                        onValueChange={(value) => {
                          if (value === PICKER_PLACEHOLDER) return;
                          const picked = leaveTypeById.get(value);
                          if (!picked) return;
                          leavesField.pushValue({
                            leaveTypeId: picked.id,
                            mode: picked.defaultMode,
                            totalDays: picked.defaultDays,
                          } satisfies UserLeaveAllowanceInput);
                        }}
                        disabled={availableLeaveTypes.length === 0}
                      >
                        <SelectTrigger className="w-64">
                          <SelectValue placeholder="Add leave…" />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value={PICKER_PLACEHOLDER} disabled>
                            Add leave…
                          </SelectItem>
                          {availableLeaveTypes.map((leaveType) => (
                            <SelectItem key={leaveType.id} value={leaveType.id}>
                              {leaveType.name}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    </div>
                  </div>
                );
              }}
            </form.Field>
          </CardContent>
        </Card>
      )}

      <form.Subscribe selector={(state) => [state.canSubmit, state.isSubmitting] as const}>
        {([canSubmit, isSubmitting]) => (
          <div className="flex gap-2">
            <Button type="submit" disabled={!canSubmit}>
              {isSubmitting ? 'Saving…' : 'Save'}
            </Button>
            <Button
              type="button"
              variant="outline"
              onClick={() => router.navigate({ to: '/admin/users' })}
              disabled={isSubmitting}
            >
              Cancel
            </Button>
          </div>
        )}
      </form.Subscribe>
    </form>
  );
}
