import { useForm } from '@tanstack/react-form';
import { useRouter } from '@tanstack/react-router';
import { toast } from 'sonner';
import { contractSchema, type ContractInput, type ContractTaskInput } from '#/features/contracts/contracts.schemas';
import type { Customer } from '#/features/customers/customers.server';
import type { User } from '#/features/users/users.server';
import { Button } from '#/components/ui/button';
import { FormCard } from '#/components/form-card';
import { Input } from '#/components/ui/input';
import { Label } from '#/components/ui/label';
import { TextField } from '#/components/text-field';
import { FieldError } from '#/components/field-error';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '#/components/ui/select';
import { formatEntityNumber } from '#/lib/utils';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '#/components/ui/table';

interface ContractFormProps {
  initial: Partial<ContractInput>;
  customers: Customer[];
  consultants: User[];
  onSubmit: (values: ContractInput) => Promise<unknown>;
  title: string;
  onDone?: () => void;
}

export function ContractForm({ initial, customers, consultants, onSubmit, title, onDone }: ContractFormProps) {
  const router = useRouter();
  const navigateOnDone = () => {
    if (onDone) {
      onDone();
      return;
    }
    router.navigate({ to: '/admin/contracts' });
  };

  const form = useForm({
    defaultValues: {
      customerId: initial.customerId ?? '',
      consultantId: initial.consultantId ?? '',
      subject: initial.subject ?? '',
      startDate: initial.startDate ?? '',
      endDate: initial.endDate ?? '',
      tasks: initial.tasks ?? [{ name: '', dayRate: 0 }],
    } satisfies ContractInput,
    validators: {
      onChange: contractSchema,
    },
    onSubmit: async ({ value }) => {
      try {
        await onSubmit(value);
        toast.success('Contract saved');
        void router.invalidate();
        navigateOnDone();
      } catch {
        toast.error('Failed to save contract');
      }
    },
  });

  return (
    <FormCard title={title} form={form} onCancel={navigateOnDone}>
      <form.Field name="customerId">
        {(field) => (
          <div className="grid gap-1.5">
            <Label htmlFor={field.name}>Customer</Label>
            <Select value={field.state.value} onValueChange={(value) => field.handleChange(value)}>
              <SelectTrigger id={field.name} onBlur={field.handleBlur}>
                <SelectValue placeholder="Select a customer" />
              </SelectTrigger>
              <SelectContent>
                {customers.map((customer) => (
                  <SelectItem key={customer.id} value={customer.id}>
                    {formatEntityNumber(customer.number)} — {customer.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <FieldError field={field} />
          </div>
        )}
      </form.Field>

      <form.Field name="consultantId">
        {(field) => (
          <div className="grid gap-1.5">
            <Label htmlFor={field.name}>Consultant</Label>
            <Select value={field.state.value} onValueChange={(value) => field.handleChange(value)}>
              <SelectTrigger id={field.name} onBlur={field.handleBlur}>
                <SelectValue placeholder="Select a consultant" />
              </SelectTrigger>
              <SelectContent>
                {consultants.map((consultant) => (
                  <SelectItem key={consultant.id} value={consultant.id}>
                    {consultant.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <FieldError field={field} />
          </div>
        )}
      </form.Field>

      <form.Field name="subject">{(field) => <TextField field={field} label="Subject" autoFocus />}</form.Field>

      <div className="grid grid-cols-2 gap-4">
        <form.Field name="startDate">
          {(field) => <TextField field={field} label="Start Date" type="date" />}
        </form.Field>
        <form.Field name="endDate">{(field) => <TextField field={field} label="End Date" type="date" />}</form.Field>
      </div>

      <div>
        <Label className="mb-2 block">Tasks</Label>
        <form.Field name="tasks" mode="array">
          {(tasksField) => (
            <div className="grid gap-2">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Name</TableHead>
                    <TableHead className="w-36">Day Rate</TableHead>
                    <TableHead className="w-10" />
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {tasksField.state.value.map((_, index) => (
                    <TableRow key={index}>
                      <TableCell className="py-1">
                        <form.Field name={`tasks[${index}].name`}>
                          {(field) => (
                            <>
                              <Input
                                id={field.name}
                                value={field.state.value}
                                onChange={(changeEvent) => field.handleChange(changeEvent.target.value)}
                                onBlur={field.handleBlur}
                                placeholder="Task name"
                              />
                              <FieldError field={field} />
                            </>
                          )}
                        </form.Field>
                      </TableCell>
                      <TableCell className="py-1">
                        <form.Field name={`tasks[${index}].dayRate`}>
                          {(field) => (
                            <>
                              <Input
                                id={field.name}
                                type="number"
                                step="0.01"
                                min="0.01"
                                value={field.state.value === 0 ? '' : String(field.state.value)}
                                onChange={(changeEvent) =>
                                  field.handleChange(parseFloat(changeEvent.target.value) || 0)
                                }
                                onBlur={field.handleBlur}
                                placeholder="0.00"
                              />
                              <FieldError field={field} />
                            </>
                          )}
                        </form.Field>
                      </TableCell>
                      <TableCell className="py-1">
                        {tasksField.state.value.length > 1 && (
                          <Button type="button" variant="ghost" size="sm" onClick={() => tasksField.removeValue(index)}>
                            ×
                          </Button>
                        )}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
              <Button
                type="button"
                variant="outline"
                size="sm"
                className="w-fit"
                onClick={() => tasksField.pushValue({ name: '', dayRate: 0 } as ContractTaskInput)}
              >
                Add task
              </Button>
              <form.Field name="tasks">{(field) => <FieldError field={field} />}</form.Field>
            </div>
          )}
        </form.Field>
      </div>
    </FormCard>
  );
}
