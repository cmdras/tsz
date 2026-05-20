import { Alert, AlertDescription, AlertTitle } from '#/components/ui/alert';

export function CustomerNotFound() {
  return (
    <div className="p-6">
      <Alert variant="destructive">
        <AlertTitle>Customer not found</AlertTitle>
        <AlertDescription>No customer exists with this ID.</AlertDescription>
      </Alert>
    </div>
  );
}
