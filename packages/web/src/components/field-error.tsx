import type { AnyFieldApi } from '@tanstack/react-form';

export function FieldError({ field }: { field: AnyFieldApi }) {
  if (!field.state.meta.isTouched || field.state.meta.errors.length === 0) return null;

  const message = field.state.meta.errors
    .map((error) => (typeof error === 'string' ? error : (error as { message?: string } | null)?.message))
    .filter(Boolean)
    .join(', ');

  if (!message) return null;
  return <p className="mt-1 text-xs text-destructive">{message}</p>;
}
