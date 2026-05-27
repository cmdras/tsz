import { createFileRoute, useRouteContext } from '@tanstack/react-router';
import { fetchHomeLeaveOverview, fetchHomeMonth } from '#/features/home/home.functions';
import { buildHomeViewModel } from '#/features/home/home-view';
import { toIsoDateString } from '#/lib/date-utils';
import { CaughtUpHero } from './index/-components/caught-up-hero';
import { Greeting } from './index/-components/greeting';
import { QuickLinks } from './index/-components/quick-links';
import { StatStrip } from './index/-components/stat-strip';

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
        <Greeting name={viewModel.greetingName} loadTime={new Date(loadTime)} />
        <div className="w-full max-w-sm">
          <CaughtUpHero />
          <StatStrip stats={viewModel.stats} />
        </div>
        <QuickLinks />
      </div>
    );
  }

  // tasks tone — task list UI added in the next slice
  return (
    <div className="flex flex-col items-center gap-8 py-12">
      <Greeting name={viewModel.greetingName} loadTime={new Date(loadTime)} />
      <div className="w-full max-w-sm">
        <StatStrip stats={viewModel.stats} />
      </div>
      <QuickLinks />
    </div>
  );
}
