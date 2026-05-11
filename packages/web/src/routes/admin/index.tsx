import { createFileRoute } from '@tanstack/react-router';
import { ComingSoon } from '#/components/coming-soon';

export const Route = createFileRoute('/admin/')({ component: () => <ComingSoon slice="S1" /> });
