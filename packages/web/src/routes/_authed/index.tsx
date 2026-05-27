import { createFileRoute, useRouteContext } from '@tanstack/react-router';
import { fetchHomeLeaveOverview, fetchHomeMonth } from '#/features/home/home.functions';
import { buildHomeViewModel } from '#/features/home/home-view';
import { toIsoDateString } from '#/lib/date-utils';
import { CaughtUpHero } from './index/-components/caught-up-hero';
import { Greeting } from './index/-components/greeting';
import { QuickLinks } from './index/-components/quick-links';
import { StatStrip } from './index/-components/stat-strip';
import { TasksHero } from './index/-components/tasks-hero';
import { TaskRow } from './index/-components/task-row';

function currentYearMonth(): string {
  const now = new Date();
  return toIsoDateString(now).slice(0, 7);
}

function currentYear(): number {
  return new Date().getFullYear();
}

export const Route = createFileRoute('/_authed/')({
  loader: async () => {
    const loadTime = new Date();
    const yearMonth = currentYearMonth();
    const year = currentYear();

    const [month, leaveOverview] = await Promise.all([
      fetchHomeMonth({ data: { yearMonth } }),
      fetchHomeLeaveOverview({ data: { year } }),
    ]);

    return { month, leaveOverview, loadTime };
  },
  component: Home,
});

function Home() {
  const { month, leaveOverview, loadTime } = Route.useLoaderData();
  const { currentUser } = useRouteContext({ from: '/_authed' });

  const greetingName = currentUser.name ?? '';
  const viewModel = buildHomeViewModel(month, leaveOverview, new Date(loadTime), greetingName);

  if (viewModel.tone === 'caughtUp') {
    return (
      <div className="flex flex-col items-center gap-8 py-12">
        <Greeting
          name={viewModel.greetingName}
          loadTime={new Date(loadTime)}
          accent={{ text: 'all caught up.', className: 'text-primary' }}
        />
        <div className="w-full max-w-sm">
          <CaughtUpHero />
          <StatStrip stats={viewModel.stats} />
        </div>
        <QuickLinks />
      </div>
    );
  }

  return (
    <div className="flex flex-col items-center gap-8 py-12">
      <Greeting
        name={viewModel.greetingName}
        loadTime={new Date(loadTime)}
        accent={{ text: `${viewModel.tasks.length} things to do.`, className: 'text-amber-400' }}
      />
      <div className="w-full max-w-sm">
        <TasksHero taskCount={viewModel.tasks.length} />
        <div className="mt-2.5 flex flex-col gap-2.5">
          {viewModel.tasks.map((task) => (
            <TaskRow key={task.weekStart} task={task} />
          ))}
        </div>
        <StatStrip stats={viewModel.stats} />
      </div>
      <QuickLinks />
    </div>
  );
}
