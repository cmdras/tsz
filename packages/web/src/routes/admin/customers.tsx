import { createFileRoute } from '@tanstack/react-router';
import { ComingSoon } from '#/components/coming-soon';

export const Route = createFileRoute('/admin/customers')({ component: () => <ComingSoon slice="S1" /> });
