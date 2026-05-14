import { createFileRoute, Link, useRouter } from '@tanstack/react-router';
import { toast } from 'sonner';
import { fetchUsers, archiveUserFn } from './-server';
import { searchSchema, roleLabels } from './-schemas';
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

export const Route = createFileRoute('/admin/users/')({
  validateSearch: searchSchema,
  loaderDeps: ({ search }) => ({
    search: search.search,
    sort: search.sort,
    page: search.page,
  }),
  loader: ({ deps }) => fetchUsers({ data: deps }),
  component: UserList,
});

function UserList() {
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
      await archiveUserFn({ data: id });
      toast.success('User archived');
      router.invalidate();
    } catch {
      toast.error('Failed to archive user');
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
        <h1 className="text-2xl font-bold">Users</h1>
        <Button asChild>
          <Link to="/admin/users/new">New user</Link>
        </Button>
      </div>

      <div className="flex items-center gap-4 mb-4">
        <Input
          placeholder="Search name, email or role…"
          defaultValue={search ?? ''}
          onChange={(changeEvent) => handleSearch(changeEvent.target.value)}
          className="max-w-xs"
        />
      </div>

      <Table>
        <TableHeader>
          <TableRow>
            <SortableHeader column="name" label="Name" active={sort} onToggle={toggleSort} />
            <SortableHeader column="email" label="Email" active={sort} onToggle={toggleSort} />
            <SortableHeader column="role" label="Role" active={sort} onToggle={toggleSort} />
            <TableHead />
          </TableRow>
        </TableHeader>
        <TableBody>
          {items.map((user) => (
            <TableRow key={user.id}>
              <TableCell>
                <Link to="/admin/users/$id" params={{ id: user.id }} className="hover:underline">
                  {user.name}
                </Link>
              </TableCell>
              <TableCell>{user.email}</TableCell>
              <TableCell>{roleLabels[user.role]}</TableCell>
              <TableCell className="text-right">
                <div className="flex gap-2 justify-end">
                  <Button size="sm" variant="outline" asChild>
                    <Link to="/admin/users/$id" params={{ id: user.id }}>
                      Edit
                    </Link>
                  </Button>
                  <AlertDialog>
                    <AlertDialogTrigger asChild>
                      <Button size="sm" variant="outline">
                        Archive
                      </Button>
                    </AlertDialogTrigger>
                    <AlertDialogContent>
                      <AlertDialogHeader>
                        <AlertDialogTitle>Archive user?</AlertDialogTitle>
                        <AlertDialogDescription>{user.name} will be removed from the user list.</AlertDialogDescription>
                      </AlertDialogHeader>
                      <AlertDialogFooter>
                        <AlertDialogCancel>Cancel</AlertDialogCancel>
                        <AlertDialogAction onClick={() => handleArchive(user.id)}>Archive</AlertDialogAction>
                      </AlertDialogFooter>
                    </AlertDialogContent>
                  </AlertDialog>
                </div>
              </TableCell>
            </TableRow>
          ))}
          {items.length === 0 && (
            <TableRow>
              <TableCell colSpan={4} className="text-center text-muted-foreground">
                No users found.
              </TableCell>
            </TableRow>
          )}
        </TableBody>
      </Table>

      <TablePagination page={page} totalPages={totalPages} total={total} onChange={goToPage} />
    </>
  );
}
