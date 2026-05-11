import { createFileRoute } from '@tanstack/react-router';
import { ComingSoon } from '#/components/coming-soon';

export const Route = createFileRoute('/admin/leave-types')({ component: () => <ComingSoon slice="S4" /> });
