import { createFileRoute } from '@tanstack/react-router';
import { ComingSoon } from '#/components/coming-soon';

export const Route = createFileRoute('/time-entry/')({ component: () => <ComingSoon slice="S7" /> });
