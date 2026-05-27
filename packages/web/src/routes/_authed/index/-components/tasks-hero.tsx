import { PencilIcon } from 'lucide-react';
import { Card } from '#/components/ui/card';

interface TasksHeroProps {
  taskCount: number;
}

export function TasksHero({ taskCount }: TasksHeroProps) {
  return (
    <div className="relative">
      {/* Corner brackets — amber accent */}
      <span
        className="pointer-events-none absolute left-0 top-0 h-6 w-6 border-l-2 border-t-2 border-amber-500"
        aria-hidden
      />
      <span
        className="pointer-events-none absolute right-0 top-0 h-6 w-6 border-r-2 border-t-2 border-amber-500"
        aria-hidden
      />
      <span
        className="pointer-events-none absolute bottom-0 left-0 h-6 w-6 border-b-2 border-l-2 border-amber-500"
        aria-hidden
      />
      <span
        className="pointer-events-none absolute bottom-0 right-0 h-6 w-6 border-b-2 border-r-2 border-amber-500"
        aria-hidden
      />

      <Card className="flex flex-col items-center gap-6 px-10 py-10">
        {/* Amber eyebrow */}
        <p className="text-xs font-semibold uppercase tracking-wider text-amber-400">
          {taskCount} open {taskCount === 1 ? 'task' : 'tasks'}
        </p>

        {/* Hero circle + count badge */}
        <div className="relative">
          {/* 128px hero circle with amber glow + centered pencil */}
          <div
            className="flex h-32 w-32 items-center justify-center rounded-full border-2 border-amber-500"
            style={{ boxShadow: '0 0 32px 8px color-mix(in oklab, var(--color-amber-500) 25%, transparent)' }}
          >
            <PencilIcon className="h-14 w-14 stroke-[1.5] text-amber-400" aria-hidden />
          </div>

          {/* Green count badge — top-right */}
          <div
            className="absolute -right-2 -top-2 flex h-[38px] w-[38px] items-center justify-center rounded-full bg-primary text-sm font-bold text-primary-foreground"
            style={{
              boxShadow: '0 0 0 2px var(--card), 0 0 12px 3px color-mix(in oklab, var(--primary) 35%, transparent)',
            }}
            aria-label={`${taskCount} open tasks`}
          >
            {taskCount}
          </div>
        </div>
      </Card>
    </div>
  );
}
