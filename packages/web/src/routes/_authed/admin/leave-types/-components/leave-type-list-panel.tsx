import { Link, useNavigate } from '@tanstack/react-router';
import { Badge } from '#/components/ui/badge';
import { Button } from '#/components/ui/button';
import { Input } from '#/components/ui/input';
import { useDebouncedCallback } from '#/hooks/use-debounced-callback';
import { cn } from '#/lib/utils';
import type { LeaveType } from '#/features/leave-types/leave-types.server';
import { PAGE_SIZE } from '#/features/leave-types/leave-types.schemas';

interface LeaveTypeListPanelProps {
  leaveTypes: LeaveType[];
  total: number;
  selectedId?: string;
  search?: string;
  page?: number;
  archived?: boolean;
}

export function LeaveTypeListPanel({
  leaveTypes,
  total,
  selectedId,
  search,
  page = 1,
  archived,
}: LeaveTypeListPanelProps) {
  const navigate = useNavigate({ from: '/admin/leave-types/' });
  const totalPages = Math.max(1, Math.ceil(total / PAGE_SIZE));

  const handleSearch = useDebouncedCallback((value: string) => {
    void navigate({ search: (previous) => ({ ...previous, search: value || undefined, page: undefined }) });
  }, 300);

  const toggleArchived = () => {
    void navigate({
      search: (previous) => ({
        ...previous,
        archived: previous.archived ? undefined : true,
        page: undefined,
      }),
    });
  };

  const goToPage = (targetPage: number) => {
    void navigate({
      search: (previous) => ({ ...previous, page: targetPage === 1 ? undefined : targetPage }),
    });
  };

  return (
    <div className="w-80 flex flex-col border rounded-lg overflow-hidden flex-shrink-0">
      <div className="p-3 border-b space-y-2">
        <Input
          placeholder="Search name…"
          defaultValue={search ?? ''}
          onChange={(changeEvent) => handleSearch(changeEvent.target.value)}
        />
        <Button variant={archived ? 'secondary' : 'outline'} size="sm" className="w-full" onClick={toggleArchived}>
          {archived ? 'Showing archived' : 'Show archived'}
        </Button>
      </div>

      <div className="flex-1 overflow-y-auto min-h-0 scrollbar-euricom">
        {leaveTypes.map((leaveType) => {
          const isSelected = leaveType.id === selectedId;
          return (
            <Link
              key={leaveType.id}
              to="/admin/leave-types/$id"
              params={{ id: leaveType.id }}
              search={(previous) => previous}
              preload={false}
              className={cn(
                'group relative flex flex-col px-3 py-2.5 hover:bg-muted/50 transition-colors',
                isSelected && 'bg-muted/30',
                leaveType.isArchived && 'opacity-60',
              )}
            >
              <span
                aria-hidden
                className={cn(
                  'absolute left-0 top-0 h-full w-0.5 bg-primary origin-center transition-transform duration-200 ease-out',
                  isSelected ? 'scale-y-100' : 'scale-y-0',
                )}
              />
              <div className="flex items-baseline justify-between gap-2">
                <span className="text-sm font-medium truncate">{leaveType.name}</span>
                {leaveType.isArchived && (
                  <Badge variant="secondary" className="text-[10px] px-1 py-0 h-4 flex-shrink-0">
                    Archived
                  </Badge>
                )}
              </div>
              <span className="text-xs text-muted-foreground mt-0.5">
                {leaveType.defaultMode === 'Unlimited' ? 'Unlimited' : `${leaveType.defaultDays} days · Limited`}
              </span>
            </Link>
          );
        })}
        {leaveTypes.length === 0 && (
          <p className="text-center text-muted-foreground text-sm py-8">No leave types found.</p>
        )}
      </div>

      <div className="px-3 py-2 border-t flex items-center justify-between text-xs text-muted-foreground">
        <span>{total} leave types</span>
        {totalPages > 1 && (
          <div className="flex items-center gap-1">
            <Button
              variant="ghost"
              size="sm"
              className="h-6 px-1.5"
              onClick={() => goToPage(page - 1)}
              disabled={page <= 1}
            >
              ‹
            </Button>
            <span>
              {page} / {totalPages}
            </span>
            <Button
              variant="ghost"
              size="sm"
              className="h-6 px-1.5"
              onClick={() => goToPage(page + 1)}
              disabled={page >= totalPages}
            >
              ›
            </Button>
          </div>
        )}
      </div>
    </div>
  );
}
