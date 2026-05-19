import { createFileRoute, Link } from '@tanstack/react-router';
import { fetchAdminStats } from '#/features/stats/stats.functions';
import { Card, CardContent, CardHeader, CardTitle } from '#/components/ui/card';

export const Route = createFileRoute('/_authed/admin/')({
  loader: () => fetchAdminStats(),
  component: AdminDashboard,
});

interface StatCardProps {
  label: string;
  count: number;
  href: string;
}

function StatCard({ label, count, href }: StatCardProps) {
  return (
    <Link to={href}>
      <Card className="hover:bg-muted/50 transition-colors cursor-pointer">
        <CardHeader className="pb-2">
          <CardTitle className="text-sm font-medium text-muted-foreground">{label}</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-3xl font-bold">{count}</p>
        </CardContent>
      </Card>
    </Link>
  );
}

function AdminDashboard() {
  const stats = Route.useLoaderData();

  return (
    <>
      <h1 className="text-2xl font-bold mb-6">Admin Dashboard</h1>
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <StatCard label="Customers" count={stats.customers} href="/admin/customers" />
        <StatCard label="Users" count={stats.users} href="/admin/users" />
        <StatCard label="Contracts" count={stats.contracts} href="/admin/contracts" />
        <StatCard label="Leave Types" count={stats.leaveTypes} href="/admin/leave-types" />
      </div>
    </>
  );
}
