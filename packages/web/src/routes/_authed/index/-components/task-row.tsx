import { Link } from '@tanstack/react-router';
import { ArrowUpRightIcon, PencilIcon } from 'lucide-react';
import { Badge } from '#/components/ui/badge';
import type { HomeWeekTask } from '#/features/home/home-view';

interface TaskRowProps {
  task: HomeWeekTask;
}

export function TaskRow({ task }: TaskRowProps) {
  const isPrimary = task.isPrimary;

  return (
    <Link
      to="/time-entry"
      search={{ week: task.weekStart }}
      className={[
        'flex items-center gap-3 rounded-lg border px-3 py-3 transition-colors hover:brightness-110',
        isPrimary ? 'border-primary bg-primary/10' : 'border-border bg-card',
      ].join(' ')}
    >
      {/* Decorative checkbox outline */}
      <span className="flex h-5 w-5 shrink-0 items-center justify-center rounded border border-border" aria-hidden />

      {/* Kind chip — amber pencil */}
      <span
        className="flex items-center gap-1 rounded-full border border-amber-500/30 bg-amber-500/10 px-2 py-0.5 text-xs text-amber-400"
        aria-hidden
      >
        <PencilIcon className="size-3" aria-hidden />
      </span>

      {/* Title + meta */}
      <div className="flex flex-1 flex-col gap-0.5 overflow-hidden">
        <span className="text-sm font-semibold text-foreground">Week {task.weekNumber}</span>
        <span className="truncate text-xs text-muted-foreground">
          {task.dateRange} · {task.loggedHours} h logged
        </span>
      </div>

      {/* Status pill */}
      {task.status === 'empty' ? (
        <Badge variant="destructive" className="shrink-0 text-[10px] uppercase tracking-wide">
          Empty
        </Badge>
      ) : (
        <Badge
          className="shrink-0 border-amber-500/30 bg-amber-500/15 text-[10px] uppercase tracking-wide text-amber-400"
          variant="outline"
        >
          Draft
        </Badge>
      )}

      {/* Open button */}
      <span
        className={[
          'flex shrink-0 items-center gap-1 rounded-md px-2 py-1 text-xs font-medium',
          isPrimary ? 'bg-primary text-primary-foreground' : 'border border-border bg-transparent text-foreground',
        ].join(' ')}
        aria-hidden
      >
        Open
        <ArrowUpRightIcon className="size-3" />
      </span>
    </Link>
  );
}
