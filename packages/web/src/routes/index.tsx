import { createFileRoute } from '@tanstack/react-router';
import { Card, CardContent, CardHeader, CardTitle } from '#/components/ui/card';

export const Route = createFileRoute('/')({ component: Home });

function Home() {
  return (
    <Card className="max-w-md">
      <CardHeader>
        <CardTitle>Welcome to Timesheet Zone</CardTitle>
      </CardHeader>
      <CardContent>Track time, manage leave, and stay on top of timesheets.</CardContent>
    </Card>
  );
}
