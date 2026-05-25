import type { ReactNode } from 'react';
import { Button } from '#/components/ui/button';
import { Input } from '#/components/ui/input';
import { ArchiveFilterTabs } from '#/components/archive-filter-tabs';
import type { ArchiveFilter } from '#/lib/archive-filter';

interface AdminListPanelProps {
  searchPlaceholder: string;
  search?: string;
  filter?: ArchiveFilter;
  total: number;
  totalLabel: string;
  page: number;
  totalPages: number;
  onSearchChange: (value: string) => void;
  onFilterChange: (value: ArchiveFilter) => void;
  onPageChange: (page: number) => void;
  emptyMessage: string;
  children: ReactNode;
}

export function AdminListPanel({
  searchPlaceholder,
  search,
  filter,
  total,
  totalLabel,
  page,
  totalPages,
  onSearchChange,
  onFilterChange,
  onPageChange,
  emptyMessage,
  children,
}: AdminListPanelProps) {
  return (
    <div className="w-80 flex flex-col border rounded-lg overflow-hidden flex-shrink-0">
      <div className="p-3 border-b space-y-2">
        <Input
          placeholder={searchPlaceholder}
          defaultValue={search ?? ''}
          onChange={(changeEvent) => onSearchChange(changeEvent.target.value)}
        />
        <ArchiveFilterTabs value={filter} onValueChange={onFilterChange} />
      </div>

      <div className="flex-1 overflow-y-auto min-h-0 scrollbar-euricom">
        {children}
        {total === 0 && <p className="text-center text-muted-foreground text-sm py-8">{emptyMessage}</p>}
      </div>

      <div className="px-3 py-2 border-t flex items-center justify-between text-xs text-muted-foreground">
        <span>
          {total} {totalLabel}
        </span>
        {totalPages > 1 && (
          <div className="flex items-center gap-1">
            <Button
              variant="ghost"
              size="sm"
              className="h-6 px-1.5"
              onClick={() => onPageChange(page - 1)}
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
              onClick={() => onPageChange(page + 1)}
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
