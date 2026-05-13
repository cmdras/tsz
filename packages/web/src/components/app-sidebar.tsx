import { Link, useLocation } from '@tanstack/react-router';
import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
} from '#/components/ui/sidebar';

type NavItem = { label: string; to: string };

const navGroups = [
  {
    label: 'Time tracking',
    items: [
      { label: 'Time entry', to: '/time-entry' },
      { label: 'Timesheets', to: '/timesheets' },
      { label: 'Leave overview', to: '/leave-overview' },
    ],
  },
  {
    label: 'Admin',
    items: [
      { label: 'Customers', to: '/admin/customers' },
      { label: 'Users', to: '/admin/users' },
      { label: 'Contracts', to: '/admin/contracts' },
      { label: 'Leave types', to: '/admin/leave-types' },
    ],
  },
] as const satisfies ReadonlyArray<{ label: string; items: ReadonlyArray<NavItem> }>;

function NavGroup({
  label,
  items,
  isActive,
}: {
  label: string;
  items: ReadonlyArray<NavItem>;
  isActive: (to: string) => boolean;
}) {
  return (
    <SidebarGroup>
      <SidebarGroupLabel>{label}</SidebarGroupLabel>
      <SidebarGroupContent>
        <SidebarMenu>
          {items.map((item) => (
            <SidebarMenuItem key={item.to}>
              <SidebarMenuButton asChild isActive={isActive(item.to)}>
                <Link to={item.to}>{item.label}</Link>
              </SidebarMenuButton>
            </SidebarMenuItem>
          ))}
        </SidebarMenu>
      </SidebarGroupContent>
    </SidebarGroup>
  );
}

export function AppSidebar() {
  const pathname = useLocation({ select: (s) => s.pathname });
  const isActive = (to: string) => pathname === to || pathname.startsWith(to + '/');

  return (
    <Sidebar>
      <SidebarContent>
        {navGroups.map((group) => (
          <NavGroup key={group.label} label={group.label} items={group.items} isActive={isActive} />
        ))}
      </SidebarContent>
    </Sidebar>
  );
}
