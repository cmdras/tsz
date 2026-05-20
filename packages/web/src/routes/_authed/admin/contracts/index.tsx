import { createFileRoute, Link, useRouter } from '@tanstack/react-router';
import { toast } from 'sonner';
import { fetchContracts, archiveContractFn, unarchiveContractFn } from '#/features/contracts/contracts.functions';
import { searchSchema } from '#/features/contracts/contracts.schemas';
import { Button } from '#/components/ui/button';
import { Input } from '#/components/ui/input';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '#/components/ui/table';
import { SortableHeader } from '#/components/sortable-header';
import { TablePagination } from '#/components/table-pagination';
import { useDebouncedCallback } from '#/hooks/use-debounced-callback';
import { formatEntityNumber } from '#/lib/utils';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from '#/components/ui/alert-dialog';

const PAGE_SIZE = 25;

export const Route = createFileRoute('/_authed/admin/contracts/')({
  validateSearch: searchSchema,
  loaderDeps: ({ search }) => ({
    search: search.search,
    sort: search.sort,
    page: search.page,
    archived: search.archived,
  }),
  loader: ({ deps }) => fetchContracts({ data: deps }),
  component: ContractList,
});

function ContractList() {
  const { items, total } = Route.useLoaderData();
  const { search, sort, page = 1, archived } = Route.useSearch();
  const router = useRouter();
  const navigate = Route.useNavigate();

  const totalPages = Math.max(1, Math.ceil(total / PAGE_SIZE));

  const handleSearch = useDebouncedCallback((value: string) => {
    navigate({ search: (previousSearch) => ({ ...previousSearch, search: value || undefined, page: undefined }) });
  }, 300);

  const toggleArchived = () => {
    navigate({
      search: (previousSearch) => ({
        ...previousSearch,
        archived: previousSearch.archived ? undefined : true,
        page: undefined,
      }),
    });
  };

  const handleArchive = async (id: string) => {
    try {
      await archiveContractFn({ data: id });
      toast.success('Contract archived');
      router.invalidate();
    } catch {
      toast.error('Failed to archive contract');
    }
  };

  const handleUnarchive = async (id: string) => {
    try {
      await unarchiveContractFn({ data: id });
      toast.success('Contract unarchived');
      router.invalidate();
    } catch {
      toast.error('Failed to unarchive contract');
    }
  };

  const toggleSort = (column: string) => {
    navigate({
      search: (previousSearch) => ({
        ...previousSearch,
        sort: previousSearch.sort === column ? `${column}-` : column,
        page: undefined,
      }),
    });
  };

  const goToPage = (targetPage: number) => {
    navigate({ search: (previousSearch) => ({ ...previousSearch, page: targetPage === 1 ? undefined : targetPage }) });
  };

  return (
    <>
      <div className="flex items-center justify-between mb-4">
        <h1 className="text-2xl font-bold">Contracts</h1>
        <Button asChild>
          <Link to="/admin/contracts/new">New contract</Link>
        </Button>
      </div>

      <div className="flex items-center gap-4 mb-4">
        <Input
          placeholder="Search subject, customer or consultant…"
          defaultValue={search ?? ''}
          onChange={(changeEvent) => handleSearch(changeEvent.target.value)}
          className="max-w-xs"
        />
        <Button variant={archived ? 'secondary' : 'outline'} size="sm" onClick={toggleArchived}>
          {archived ? 'Hiding archived' : 'Show archived'}
        </Button>
      </div>

      <Table>
        <TableHeader>
          <TableRow>
            <SortableHeader column="number" label="Number" active={sort} onToggle={toggleSort} />
            <SortableHeader column="customer" label="Customer" active={sort} onToggle={toggleSort} />
            <SortableHeader column="subject" label="Subject" active={sort} onToggle={toggleSort} />
            <SortableHeader column="consultant" label="Consultant" active={sort} onToggle={toggleSort} />
            <SortableHeader column="startdate" label="Start Date" active={sort} onToggle={toggleSort} />
            <SortableHeader column="enddate" label="End Date" active={sort} onToggle={toggleSort} />
            <TableHead />
          </TableRow>
        </TableHeader>
        <TableBody>
          {items.map((contract) => (
            <TableRow key={contract.id} className={contract.isArchived ? 'opacity-50' : undefined}>
              <TableCell className="font-mono">
                <Link to="/admin/contracts/$id" params={{ id: contract.id }} className="hover:underline">
                  {formatEntityNumber(contract.number)}
                </Link>
              </TableCell>
              <TableCell>{contract.customerName}</TableCell>
              <TableCell>
                <Link to="/admin/contracts/$id" params={{ id: contract.id }} className="hover:underline">
                  {contract.subject}
                </Link>
              </TableCell>
              <TableCell>{contract.consultantName}</TableCell>
              <TableCell>{contract.startDate}</TableCell>
              <TableCell>{contract.endDate ?? '—'}</TableCell>
              <TableCell className="text-right">
                <div className="flex gap-2 justify-end">
                  <Button size="sm" variant="outline" asChild>
                    <Link to="/admin/contracts/$id" params={{ id: contract.id }}>
                      Edit
                    </Link>
                  </Button>
                  {contract.isArchived ? (
                    <Button size="sm" variant="outline" onClick={() => handleUnarchive(contract.id)}>
                      Unarchive
                    </Button>
                  ) : (
                    <AlertDialog>
                      <AlertDialogTrigger asChild>
                        <Button size="sm" variant="outline">
                          Archive
                        </Button>
                      </AlertDialogTrigger>
                      <AlertDialogContent>
                        <AlertDialogHeader>
                          <AlertDialogTitle>Archive contract?</AlertDialogTitle>
                          <AlertDialogDescription>
                            {contract.subject} will be removed from the active contract list.
                          </AlertDialogDescription>
                        </AlertDialogHeader>
                        <AlertDialogFooter>
                          <AlertDialogCancel>Cancel</AlertDialogCancel>
                          <AlertDialogAction onClick={() => handleArchive(contract.id)}>Archive</AlertDialogAction>
                        </AlertDialogFooter>
                      </AlertDialogContent>
                    </AlertDialog>
                  )}
                </div>
              </TableCell>
            </TableRow>
          ))}
          {items.length === 0 && (
            <TableRow>
              <TableCell colSpan={7} className="text-center text-muted-foreground">
                No contracts found.
              </TableCell>
            </TableRow>
          )}
        </TableBody>
      </Table>

      <TablePagination page={page} totalPages={totalPages} total={total} onChange={goToPage} />
    </>
  );
}
