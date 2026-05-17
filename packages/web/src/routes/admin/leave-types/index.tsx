import { createFileRoute, Link, useRouter } from '@tanstack/react-router';
import { toast } from 'sonner';
import {
  fetchLeaveTypes,
  archiveLeaveTypeFn,
  unarchiveLeaveTypeFn,
} from '#/features/leave-types/leave-types.functions';
import { searchSchema } from '#/features/leave-types/leave-types.schemas';
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

export const Route = createFileRoute('/admin/leave-types/')({
  validateSearch: searchSchema,
  loaderDeps: ({ search }) => ({
    search: search.search,
    sort: search.sort,
    page: search.page,
    archived: search.archived,
  }),
  loader: ({ deps }) => fetchLeaveTypes({ data: deps }),
  component: LeaveTypeList,
});

function LeaveTypeList() {
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
      await archiveLeaveTypeFn({ data: id });
      toast.success('Leave type archived');
      router.invalidate();
    } catch {
      toast.error('Failed to archive leave type');
    }
  };

  const handleUnarchive = async (id: string) => {
    try {
      await unarchiveLeaveTypeFn({ data: id });
      toast.success('Leave type unarchived');
      router.invalidate();
    } catch {
      toast.error('Failed to unarchive leave type');
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
        <h1 className="text-2xl font-bold">Leave Types</h1>
        <Button asChild>
          <Link to="/admin/leave-types/new">New leave type</Link>
        </Button>
      </div>

      <div className="flex items-center gap-4 mb-4">
        <Input
          placeholder="Search name…"
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
            <SortableHeader column="name" label="Name" active={sort} onToggle={toggleSort} />
            <SortableHeader column="defaultdays" label="Default Days" active={sort} onToggle={toggleSort} />
            <TableHead />
          </TableRow>
        </TableHeader>
        <TableBody>
          {items.map((leaveType) => (
            <TableRow key={leaveType.id} className={leaveType.isArchived ? 'opacity-50' : undefined}>
              <TableCell>
                <Link to="/admin/leave-types/$id" params={{ id: leaveType.id }} className="hover:underline">
                  {leaveType.name}
                </Link>
              </TableCell>
              <TableCell>{leaveType.defaultDays}</TableCell>
              <TableCell className="text-right">
                <div className="flex gap-2 justify-end">
                  <Button size="sm" variant="outline" asChild>
                    <Link to="/admin/leave-types/$id" params={{ id: leaveType.id }}>
                      Edit
                    </Link>
                  </Button>
                  {leaveType.isArchived ? (
                    <Button size="sm" variant="outline" onClick={() => handleUnarchive(leaveType.id)}>
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
                          <AlertDialogTitle>Archive leave type?</AlertDialogTitle>
                          <AlertDialogDescription>
                            {leaveType.name} will be removed from the active leave type list.
                          </AlertDialogDescription>
                        </AlertDialogHeader>
                        <AlertDialogFooter>
                          <AlertDialogCancel>Cancel</AlertDialogCancel>
                          <AlertDialogAction onClick={() => handleArchive(leaveType.id)}>Archive</AlertDialogAction>
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
              <TableCell colSpan={3} className="text-center text-muted-foreground">
                No leave types found.
              </TableCell>
            </TableRow>
          )}
        </TableBody>
      </Table>

      <TablePagination page={page} totalPages={totalPages} total={total} onChange={goToPage} />
    </>
  );
}
