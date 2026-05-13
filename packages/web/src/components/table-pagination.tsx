import {
  Pagination,
  PaginationContent,
  PaginationItem,
  PaginationNext,
  PaginationPrevious,
} from '#/components/ui/pagination';

interface TablePaginationProps {
  page: number;
  totalPages: number;
  total: number;
  onChange: (page: number) => void;
}

export function TablePagination({ page, totalPages, total, onChange }: TablePaginationProps) {
  const prevDisabled = page <= 1;
  const nextDisabled = page >= totalPages;
  return (
    <div className="flex items-center justify-between mt-4">
      <span className="text-sm text-muted-foreground">
        Page {page} of {totalPages} · {total} total
      </span>
      <Pagination className="mx-0 w-auto justify-end">
        <PaginationContent>
          <PaginationItem>
            <PaginationPrevious
              href="#"
              aria-disabled={prevDisabled}
              className={prevDisabled ? 'pointer-events-none opacity-50' : undefined}
              onClick={(e) => {
                e.preventDefault();
                if (!prevDisabled) onChange(page - 1);
              }}
            />
          </PaginationItem>
          <PaginationItem>
            <PaginationNext
              href="#"
              aria-disabled={nextDisabled}
              className={nextDisabled ? 'pointer-events-none opacity-50' : undefined}
              onClick={(e) => {
                e.preventDefault();
                if (!nextDisabled) onChange(page + 1);
              }}
            />
          </PaginationItem>
        </PaginationContent>
      </Pagination>
    </div>
  );
}
