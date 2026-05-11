import { createFileRoute } from '@tanstack/react-router';
import { ComingSoon } from '#/components/coming-soon';

export const Route = createFileRoute('/timesheets/')({ component: () => <ComingSoon slice="S10" /> });
