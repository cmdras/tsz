import { createFileRoute } from '@tanstack/react-router';
import { ComingSoon } from '#/components/coming-soon';

export const Route = createFileRoute('/leave-overview/')({ component: () => <ComingSoon slice="S11" /> });
