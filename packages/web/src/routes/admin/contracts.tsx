import { createFileRoute } from '@tanstack/react-router';
import { ComingSoon } from '#/components/coming-soon';

export const Route = createFileRoute('/admin/contracts')({ component: () => <ComingSoon slice="S3" /> });
