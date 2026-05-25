import { Link } from '@tanstack/react-router';
import { Badge } from '#/components/ui/badge';
import { AdminListPanel } from '#/components/admin-list-panel';
import { useAdminListPanelNavigation } from '#/hooks/use-admin-list-panel-navigation';
import { cn } from '#/lib/utils';
import type { LeaveType } from '#/features/leave-types/leave-types.server';
import type { ArchiveFilter } from '#/lib/archive-filter';
import { PAGE_SIZE } from '#/features/leave-types/leave-types.schemas';

interface LeaveTypeListPanelProps {
  leaveTypes: LeaveType[];
  total: number;
  selectedId?: string;
  search?: string;
  page?: number;
  filter?: ArchiveFilter;
}

interface LeaveTypeListItemProps {
  leaveType: LeaveType;
  isSelected: boolean;
}

function buildLeaveTypeRowClassName(isSelected: boolean, isArchived: boolean) {
  return cn(
    'group relative flex flex-col px-3 py-2.5 hover:bg-muted/50 transition-colors',
    isSelected && 'bg-muted/30',
    isArchived && 'opacity-60',
  );
}

function buildIndicatorClassName(isSelected: boolean) {
  return cn(
    'absolute left-0 top-0 h-full w-0.5 bg-primary origin-center transition-transform duration-200 ease-out',
    isSelected ? 'scale-y-100' : 'scale-y-0',
  );
}

function formatLeaveTypeMode(defaultMode: LeaveType['defaultMode'], defaultDays: number) {
  return defaultMode === 'Unlimited' ? 'Unlimited' : `${defaultDays} days · Limited`;
}

function LeaveTypeListItem({ leaveType, isSelected }: LeaveTypeListItemProps) {
  return (
    <Link
      to="/admin/leave-types/$id"
      params={{ id: leaveType.id }}
      search={(previous) => previous}
      preload={false}
      className={buildLeaveTypeRowClassName(isSelected, leaveType.isArchived)}
    >
      <span aria-hidden className={buildIndicatorClassName(isSelected)} />
      <div className="flex items-baseline justify-between gap-2">
        <span className="text-sm font-medium truncate">{leaveType.name}</span>
        {leaveType.isArchived && (
          <Badge variant="secondary" className="text-[10px] px-1 py-0 h-4 flex-shrink-0">
            Archived
          </Badge>
        )}
      </div>
      <span className="text-xs text-muted-foreground mt-0.5">
        {formatLeaveTypeMode(leaveType.defaultMode, leaveType.defaultDays)}
      </span>
    </Link>
  );
}

export function LeaveTypeListPanel({
  leaveTypes,
  total,
  selectedId,
  search,
  page = 1,
  filter,
}: LeaveTypeListPanelProps) {
  const { totalPages, handleSearch, handleFilterChange, goToPage } = useAdminListPanelNavigation({
    from: '/admin/leave-types/',
    pageSize: PAGE_SIZE,
    total,
  });

  return (
    <AdminListPanel
      searchPlaceholder="Search name…"
      search={search}
      filter={filter}
      total={total}
      totalLabel="leave types"
      page={page}
      totalPages={totalPages}
      onSearchChange={handleSearch}
      onFilterChange={handleFilterChange}
      onPageChange={goToPage}
      emptyMessage="No leave types found."
    >
      {leaveTypes.map((leaveType) => (
        <LeaveTypeListItem key={leaveType.id} leaveType={leaveType} isSelected={leaveType.id === selectedId} />
      ))}
    </AdminListPanel>
  );
}
