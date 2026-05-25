import { useStore } from '@tanstack/react-form';
import type { ReadonlyStore } from '@tanstack/store';
import { Card, CardContent, CardHeader, CardTitle } from '#/components/ui/card';
import { FormFooter } from '#/components/form-footer';

interface FormLike {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  store: ReadonlyStore<any>;
  handleSubmit(): Promise<void>;
}

interface FormCardProps {
  title: string;
  form: FormLike;
  onCancel: () => void;
  children: React.ReactNode;
}

export function FormCard({ title, form, onCancel, children }: FormCardProps) {
  const canSubmit = useStore(form.store, (state: { canSubmit: boolean }) => state.canSubmit);
  const isSubmitting = useStore(form.store, (state: { isSubmitting: boolean }) => state.isSubmitting);

  return (
    <Card>
      <CardHeader>
        <CardTitle>{title}</CardTitle>
      </CardHeader>
      <CardContent>
        <form
          onSubmit={(submitEvent) => {
            submitEvent.preventDefault();
            submitEvent.stopPropagation();
            void form.handleSubmit();
          }}
          className="grid gap-4"
        >
          {children}
          <FormFooter canSubmit={canSubmit} isPending={isSubmitting} onCancel={onCancel} />
        </form>
      </CardContent>
    </Card>
  );
}
