import { CheckIcon, SparklesIcon } from 'lucide-react';
import { Card } from '#/components/ui/card';

export function CaughtUpHero() {
  return (
    <div className="relative">
      {/* Corner brackets */}
      <span
        className="pointer-events-none absolute left-0 top-0 h-6 w-6 border-l-2 border-t-2 border-primary"
        aria-hidden
      />
      <span
        className="pointer-events-none absolute right-0 top-0 h-6 w-6 border-r-2 border-t-2 border-primary"
        aria-hidden
      />
      <span
        className="pointer-events-none absolute bottom-0 left-0 h-6 w-6 border-b-2 border-l-2 border-primary"
        aria-hidden
      />
      <span
        className="pointer-events-none absolute bottom-0 right-0 h-6 w-6 border-b-2 border-r-2 border-primary"
        aria-hidden
      />

      <Card className="flex flex-col items-center gap-6 px-10 py-10">
        {/* Eyebrow */}
        <p className="text-xs font-semibold uppercase tracking-wider text-primary">No tasks · everything submitted</p>

        {/* Sparkle + hero circle + sparkle row */}
        <div className="flex items-center gap-6">
          <SparklesIcon className="h-5 w-5 text-primary opacity-60" aria-hidden />

          {/* 144 px hero circle */}
          <div
            className="flex h-36 w-36 items-center justify-center rounded-full border-2 border-primary"
            style={{ boxShadow: '0 0 32px 8px color-mix(in oklab, var(--primary) 25%, transparent)' }}
          >
            <CheckIcon className="h-16 w-16 stroke-[1.5] text-primary" aria-hidden />
          </div>

          <SparklesIcon className="h-5 w-5 text-primary opacity-60" aria-hidden />
        </div>
      </Card>
    </div>
  );
}
