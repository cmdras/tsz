import { createFileRoute, Link, useRouter } from '@tanstack/react-router';
import { toast } from 'sonner';
import { fetchCustomers, archiveCustomerFn } from './-server';
import { searchSchema } from './-schemas';
import { Button } from '#/components/ui/button';
import { Input } from '#/components/ui/input';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '#/components/ui/table';
import { SortableHeader } from '#/components/sortable-header';
import { TablePagination } from '#/components/table-pagination';
import { useDebouncedCallback } from '#/hooks/use-debounced-callback';
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

export const Route = createFileRoute('/admin/customers/')({
  validateSearch: searchSchema,
  loaderDeps: ({ search }) => ({
    search: search.search,
    sort: search.sort,
    page: search.page,
  }),
  loader: ({ deps }) => fetchCustomers({ data: deps }),
  component: CustomerList,
});

function CustomerList() {
  const { items, total } = Route.useLoaderData();
  const { search, sort, page = 1 } = Route.useSearch();
  const router = useRouter();
  const navigate = Route.useNavigate();

  const totalPages = Math.max(1, Math.ceil(total / PAGE_SIZE));

  const handleSearch = useDebouncedCallback((value: string) => {
    navigate({ search: (previousSearch) => ({ ...previousSearch, search: value || undefined, page: undefined }) });
  }, 300);

  const handleArchive = async (id: string) => {
    try {
      await archiveCustomerFn({ data: id });
      toast.success('Customer archived');
      router.invalidate();
    } catch {
      toast.error('Failed to archive customer');
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
        <h1 className="text-2xl font-bold">Customers</h1>
        <Button asChild>
          <Link to="/admin/customers/new">New customer</Link>
        </Button>
      </div>

      <div className="flex items-center gap-4 mb-4">
        <Input
          placeholder="Search name or contact…"
          defaultValue={search ?? ''}
          onChange={(changeEvent) => handleSearch(changeEvent.target.value)}
          className="max-w-xs"
        />
      </div>

      <Table>
        <TableHeader>
          <TableRow>
            <SortableHeader column="number" label="Number" active={sort} onToggle={toggleSort} />
            <SortableHeader column="name" label="Name" active={sort} onToggle={toggleSort} />
            <SortableHeader column="contact" label="Contact" active={sort} onToggle={toggleSort} />
            <TableHead>Email</TableHead>
            <SortableHeader column="city" label="City" active={sort} onToggle={toggleSort} />
            <TableHead>Country</TableHead>
            <TableHead />
          </TableRow>
        </TableHeader>
        <TableBody>
          {items.map((customer) => (
            <TableRow key={customer.id}>
              <TableCell className="font-mono">
                <Link to="/admin/customers/$id" params={{ id: customer.id }} className="hover:underline">
                  {String(customer.number).padStart(6, '0')}
                </Link>
              </TableCell>
              <TableCell>
                <Link to="/admin/customers/$id" params={{ id: customer.id }} className="hover:underline">
                  {customer.name}
                </Link>
              </TableCell>
              <TableCell>{customer.contactName}</TableCell>
              <TableCell>{customer.contactEmail}</TableCell>
              <TableCell>{customer.city}</TableCell>
              <TableCell>{customer.country}</TableCell>
              <TableCell className="text-right">
                <AlertDialog>
                  <AlertDialogTrigger asChild>
                    <Button size="sm" variant="outline">
                      Archive
                    </Button>
                  </AlertDialogTrigger>
                  <AlertDialogContent>
                    <AlertDialogHeader>
                      <AlertDialogTitle>Archive customer?</AlertDialogTitle>
                      <AlertDialogDescription>
                        {customer.name} will be removed from the customer list.
                      </AlertDialogDescription>
                    </AlertDialogHeader>
                    <AlertDialogFooter>
                      <AlertDialogCancel>Cancel</AlertDialogCancel>
                      <AlertDialogAction onClick={() => handleArchive(customer.id)}>Archive</AlertDialogAction>
                    </AlertDialogFooter>
                  </AlertDialogContent>
                </AlertDialog>
              </TableCell>
            </TableRow>
          ))}
          {items.length === 0 && (
            <TableRow>
              <TableCell colSpan={7} className="text-center text-muted-foreground">
                No customers found.
              </TableCell>
            </TableRow>
          )}
        </TableBody>
      </Table>

      <TablePagination page={page} totalPages={totalPages} total={total} onChange={goToPage} />
    </>
  );
}
