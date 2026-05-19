import { createFileRoute } from '@tanstack/react-router';
import { ComingSoon } from '#/components/coming-soon';

export const Route = createFileRoute('/_authed/time-entry/')({ component: () => <ComingSoon slice="S7" /> });
