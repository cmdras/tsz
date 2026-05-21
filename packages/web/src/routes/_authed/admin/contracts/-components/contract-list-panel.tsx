import { Link, useNavigate } from '@tanstack/react-router';
import { Badge } from '#/components/ui/badge';
import { Button } from '#/components/ui/button';
import { Input } from '#/components/ui/input';
import { useDebouncedCallback } from '#/hooks/use-debounced-callback';
import { cn, formatEntityNumber } from '#/lib/utils';
import type { Contract } from '#/features/contracts/contracts.server';
import { PAGE_SIZE } from '#/features/contracts/contracts.schemas';

interface ContractListPanelProps {
  contracts: Contract[];
  total: number;
  selectedId?: string;
  search?: string;
  page?: number;
  archived?: boolean;
}

export function ContractListPanel({
  contracts,
  total,
  selectedId,
  search,
  page = 1,
  archived,
}: ContractListPanelProps) {
  const navigate = useNavigate();
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
          placeholder="Search subject or customer…"
          defaultValue={search ?? ''}
          onChange={(changeEvent) => handleSearch(changeEvent.target.value)}
        />
        <Button variant={archived ? 'secondary' : 'outline'} size="sm" className="w-full" onClick={toggleArchived}>
          {archived ? 'Showing archived' : 'Show archived'}
        </Button>
      </div>

      <div className="flex-1 overflow-y-auto min-h-0 scrollbar-euricom">
        {contracts.map((contract) => {
          const isSelected = contract.id === selectedId;
          return (
            <Link
              key={contract.id}
              to="/admin/contracts/$id"
              params={{ id: contract.id }}
              search={(previous) => previous}
              preload={false}
              className={cn(
                'group relative flex flex-col px-3 py-2.5 hover:bg-muted/50 transition-colors',
                isSelected && 'bg-muted/30',
                contract.isArchived && 'opacity-60',
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
                <span className="text-sm font-medium truncate">{contract.customerName}</span>
                <span className="text-xs text-muted-foreground flex-shrink-0 whitespace-nowrap">
                  {contract.startDate.slice(0, 7)}
                  {' – '}
                  {contract.endDate ? contract.endDate.slice(0, 7) : '…'}
                </span>
              </div>
              <div className="flex items-center gap-1 mt-0.5">
                <span className="text-xs text-muted-foreground font-mono flex-shrink-0">
                  #{formatEntityNumber(contract.number)}
                </span>
                <span className="text-xs text-muted-foreground truncate">· {contract.subject}</span>
                {contract.isArchived && (
                  <Badge variant="secondary" className="text-[10px] px-1 py-0 h-4 flex-shrink-0">
                    Archived
                  </Badge>
                )}
              </div>
            </Link>
          );
        })}
        {contracts.length === 0 && (
          <p className="text-center text-muted-foreground text-sm py-8">No contracts found.</p>
        )}
      </div>

      <div className="px-3 py-2 border-t flex items-center justify-between text-xs text-muted-foreground">
        <span>{total} contracts</span>
        {totalPages > 1 && (
          <div className="flex items-center gap-1">
            <button
              onClick={() => goToPage(page - 1)}
              disabled={page <= 1}
              className="px-1.5 py-0.5 rounded hover:bg-muted disabled:opacity-50"
            >
              ‹
            </button>
            <span>
              {page} / {totalPages}
            </span>
            <button
              onClick={() => goToPage(page + 1)}
              disabled={page >= totalPages}
              className="px-1.5 py-0.5 rounded hover:bg-muted disabled:opacity-50"
            >
              ›
            </button>
          </div>
        )}
      </div>
    </div>
  );
}
