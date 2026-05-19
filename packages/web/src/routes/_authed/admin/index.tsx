import { createFileRoute } from '@tanstack/react-router';
import { ComingSoon } from '#/components/coming-soon';

export const Route = createFileRoute('/_authed/admin/')({ component: () => <ComingSoon slice="S1" /> });
