import { Alert, AlertDescription, AlertTitle } from '#/components/ui/alert';

export function UserNotFound() {
  return (
    <div className="p-6">
      <Alert variant="destructive">
        <AlertTitle>User not found</AlertTitle>
        <AlertDescription>No user exists with this ID.</AlertDescription>
      </Alert>
    </div>
  );
}
