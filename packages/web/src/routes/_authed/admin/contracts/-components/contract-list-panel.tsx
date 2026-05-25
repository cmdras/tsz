import { Link } from '@tanstack/react-router';
import { Badge } from '#/components/ui/badge';
import { AdminListPanel } from '#/components/admin-list-panel';
import { useAdminListPanelNavigation } from '#/hooks/use-admin-list-panel-navigation';
import { cn, formatEntityNumber } from '#/lib/utils';
import type { Contract } from '#/features/contracts/contracts.server';
import type { ArchiveFilter } from '#/lib/archive-filter';
import { PAGE_SIZE } from '#/features/contracts/contracts.schemas';

interface ContractListPanelProps {
  contracts: Contract[];
  total: number;
  selectedId?: string;
  search?: string;
  page?: number;
  filter?: ArchiveFilter;
}

interface ContractListItemProps {
  contract: Contract;
  isSelected: boolean;
}

function buildContractRowClassName(isSelected: boolean, isArchived: boolean) {
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

function formatContractEndDate(endDate: string | null) {
  return endDate ? endDate.slice(0, 7) : '…';
}

function ContractListItem({ contract, isSelected }: ContractListItemProps) {
  return (
    <Link
      to="/admin/contracts/$id"
      params={{ id: contract.id }}
      search={(previous) => previous}
      preload={false}
      className={buildContractRowClassName(isSelected, contract.isArchived)}
    >
      <span aria-hidden className={buildIndicatorClassName(isSelected)} />
      <div className="flex items-baseline justify-between gap-2">
        <span className="text-sm font-medium truncate">{contract.customerName}</span>
        <span className="text-xs text-muted-foreground flex-shrink-0 whitespace-nowrap">
          {contract.startDate.slice(0, 7)}
          {' – '}
          {formatContractEndDate(contract.endDate)}
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
}

export function ContractListPanel({ contracts, total, selectedId, search, page = 1, filter }: ContractListPanelProps) {
  const { totalPages, handleSearch, handleFilterChange, goToPage } = useAdminListPanelNavigation({
    from: '/admin/contracts/',
    pageSize: PAGE_SIZE,
    total,
  });

  return (
    <AdminListPanel
      searchPlaceholder="Search subject or customer…"
      search={search}
      filter={filter}
      total={total}
      totalLabel="contracts"
      page={page}
      totalPages={totalPages}
      onSearchChange={handleSearch}
      onFilterChange={handleFilterChange}
      onPageChange={goToPage}
      emptyMessage="No contracts found."
    >
      {contracts.map((contract) => (
        <ContractListItem key={contract.id} contract={contract} isSelected={contract.id === selectedId} />
      ))}
    </AdminListPanel>
  );
}
