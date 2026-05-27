import { Link } from '@tanstack/react-router';
import { Button } from '#/components/ui/button';

export function QuickLinks() {
  return (
    <div className="flex justify-center gap-3">
      <Button variant="outline" asChild>
        <Link to="/time-entry">Time entry</Link>
      </Button>
      <Button variant="outline" asChild>
        <Link to="/timesheets">Timesheets</Link>
      </Button>
      <Button variant="outline" asChild>
        <Link to="/leave-overview">Leave overview</Link>
      </Button>
    </div>
  );
}
