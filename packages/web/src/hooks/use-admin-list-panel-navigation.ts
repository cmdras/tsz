import { useNavigate } from '@tanstack/react-router';
import { useDebouncedCallback } from '#/hooks/use-debounced-callback';
import type { ArchiveFilter } from '#/lib/archive-filter';

type AdminListPanelRoute = '/admin/contracts/' | '/admin/leave-types/';

interface UseAdminListPanelNavigationOptions {
  from: AdminListPanelRoute;
  pageSize: number;
  total: number;
}

export function useAdminListPanelNavigation({ from, pageSize, total }: UseAdminListPanelNavigationOptions) {
  const navigate = useNavigate({ from });
  const totalPages = Math.max(1, Math.ceil(total / pageSize));

  const handleSearch = useDebouncedCallback((value: string) => {
    void navigate({ search: (previous) => ({ ...previous, search: value || undefined, page: undefined }) });
  }, 300);

  const handleFilterChange = (value: ArchiveFilter) => {
    void navigate({ search: (previous) => ({ ...previous, filter: value, page: undefined }) });
  };

  const goToPage = (targetPage: number) => {
    void navigate({
      search: (previous) => ({ ...previous, page: targetPage === 1 ? undefined : targetPage }),
    });
  };

  return { totalPages, handleSearch, handleFilterChange, goToPage };
}
