import { createFileRoute, Link, useRouter } from '@tanstack/react-router';
import { useCallback, useEffect, useRef } from 'react';
import { toast } from 'sonner';
import { fetchCustomers, archiveCustomerFn } from './-server';
import { searchSchema, type SortColumn } from './-schemas';
import { Button } from '#/components/ui/button';
import { Input } from '#/components/ui/input';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '#/components/ui/table';
import { SortableHeader } from '#/components/sortable-header';
import { TablePagination } from '#/components/table-pagination';
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
    dir: search.dir,
    page: search.page,
  }),
  loader: ({ deps }) => fetchCustomers({ data: deps }),
  component: CustomerList,
});

function CustomerList() {
  const { items, total } = Route.useLoaderData();
  const { search, sort = 'Number', dir = 'Asc', page = 1 } = Route.useSearch();
  const router = useRouter();
  const navigate = Route.useNavigate();
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  const totalPages = Math.max(1, Math.ceil(total / PAGE_SIZE));

  const handleSearch = useCallback(
    (value: string) => {
      if (debounceRef.current) clearTimeout(debounceRef.current);
      debounceRef.current = setTimeout(() => {
        navigate({ search: (prev) => ({ ...prev, search: value || undefined, page: undefined }) });
      }, 300);
    },
    [navigate],
  );

  useEffect(
    () => () => {
      if (debounceRef.current) clearTimeout(debounceRef.current);
    },
    [],
  );

  const handleArchive = async (id: string) => {
    try {
      await archiveCustomerFn({ data: id });
      toast.success('Customer archived');
      router.invalidate();
    } catch {
      toast.error('Failed to archive customer');
    }
  };

  const toggleSort = (column: SortColumn) => {
    navigate({
      search: (prev) => ({
        ...prev,
        sort: column,
        dir: prev.sort === column && prev.dir === 'Asc' ? 'Desc' : 'Asc',
        page: undefined,
      }),
    });
  };

  const goToPage = (next: number) => {
    navigate({ search: (prev) => ({ ...prev, page: next === 1 ? undefined : next }) });
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
          onChange={(e) => handleSearch(e.target.value)}
          className="max-w-xs"
        />
      </div>

      <Table>
        <TableHeader>
          <TableRow>
            <SortableHeader column="Number" label="Number" active={sort} dir={dir} onToggle={toggleSort} />
            <SortableHeader column="Name" label="Name" active={sort} dir={dir} onToggle={toggleSort} />
            <SortableHeader column="ContactName" label="Contact" active={sort} dir={dir} onToggle={toggleSort} />
            <TableHead>Email</TableHead>
            <SortableHeader column="City" label="City" active={sort} dir={dir} onToggle={toggleSort} />
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
