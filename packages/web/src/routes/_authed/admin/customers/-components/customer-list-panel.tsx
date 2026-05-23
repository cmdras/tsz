import { Link, useNavigate } from '@tanstack/react-router';
import { Input } from '#/components/ui/input';
import { Badge } from '#/components/ui/badge';
import { ArchiveFilterTabs } from '#/components/archive-filter-tabs';
import { useDebouncedCallback } from '#/hooks/use-debounced-callback';
import { cn, formatEntityNumber, getAvatarColor } from '#/lib/utils';
import type { Customer } from '#/features/customers/customers.server';
import type { ArchiveFilter } from '#/lib/archive-filter';

interface CustomerListPanelProps {
  customers: Customer[];
  selectedId?: string;
  search?: string;
  filter?: ArchiveFilter;
}

export function CustomerListPanel({ customers, selectedId, search, filter }: CustomerListPanelProps) {
  const navigate = useNavigate({ from: '/admin/customers/' });

  const handleSearch = useDebouncedCallback((value: string) => {
    void navigate({ search: (previous) => ({ ...previous, search: value || undefined }) });
  }, 300);

  const handleFilterChange = (value: ArchiveFilter) => {
    void navigate({ search: (previous) => ({ ...previous, filter: value }) });
  };

  return (
    <div className="w-80 flex flex-col border rounded-lg overflow-hidden flex-shrink-0">
      <div className="p-3 border-b space-y-2">
        <Input
          placeholder="Search name or contact…"
          defaultValue={search ?? ''}
          onChange={(changeEvent) => handleSearch(changeEvent.target.value)}
        />
        <ArchiveFilterTabs value={filter} onValueChange={handleFilterChange} />
      </div>

      <div className="flex-1 overflow-y-auto min-h-0 scrollbar-euricom">
        {customers.map((customer) => {
          const isSelected = customer.id === selectedId;
          return (
            <Link
              key={customer.id}
              to="/admin/customers/$id"
              params={{ id: customer.id }}
              search={(previous) => previous}
              preload={false}
              className={cn(
                'group relative flex items-center gap-3 px-3 py-2.5 hover:bg-muted/50 transition-colors',
                isSelected && 'bg-muted/30',
              )}
            >
              <span
                aria-hidden
                className={cn(
                  'absolute left-0 top-0 h-full w-0.5 bg-primary origin-center transition-transform duration-200 ease-out',
                  isSelected ? 'scale-y-100' : 'scale-y-0',
                )}
              />
              <div
                className="w-8 h-8 rounded-full flex items-center justify-center text-xs font-semibold flex-shrink-0 text-white transition-transform duration-200 ease-out group-hover:scale-105"
                style={{ backgroundColor: getAvatarColor(customer.name) }}
              >
                {customer.name.slice(0, 2).toUpperCase()}
              </div>
              <div className="flex-1 min-w-0">
                <div className="flex items-center gap-1.5">
                  <span className="text-sm font-medium truncate">{customer.name}</span>
                  {customer.isArchived && (
                    <Badge variant="secondary" className="text-[10px] px-1 py-0 h-4 flex-shrink-0">
                      Archived
                    </Badge>
                  )}
                </div>
                <p className="text-xs text-muted-foreground truncate">
                  #{formatEntityNumber(customer.number)} · {customer.city}
                </p>
              </div>
            </Link>
          );
        })}
        {customers.length === 0 && (
          <p className="text-center text-muted-foreground text-sm py-8">No customers found.</p>
        )}
      </div>

      <div className="px-3 py-2 border-t text-xs text-muted-foreground">{customers.length} customers</div>
    </div>
  );
}
