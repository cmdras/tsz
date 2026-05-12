import type { AnyFieldApi } from '@tanstack/react-form';
import { Input } from '#/components/ui/input';
import { Label } from '#/components/ui/label';
import { FieldError } from '#/components/field-error';

interface TextFieldProps {
  field: AnyFieldApi;
  label: string;
  type?: React.HTMLInputTypeAttribute;
  autoFocus?: boolean;
}

export function TextField({ field, label, type, autoFocus }: TextFieldProps) {
  return (
    <div className="grid gap-2">
      <Label htmlFor={field.name}>{label}</Label>
      <Input
        id={field.name}
        name={field.name}
        type={type}
        value={field.state.value}
        onBlur={field.handleBlur}
        onChange={(e) => field.handleChange(e.target.value)}
        autoFocus={autoFocus}
      />
      <FieldError field={field} />
    </div>
  );
}
