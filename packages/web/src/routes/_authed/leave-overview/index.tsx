import { createFileRoute } from '@tanstack/react-router';
import { ComingSoon } from '#/components/coming-soon';

export const Route = createFileRoute('/_authed/leave-overview/')({ component: () => <ComingSoon slice="S11" /> });
