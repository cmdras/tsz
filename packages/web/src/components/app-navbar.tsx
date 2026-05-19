import { Link, useLocation, useRouteContext } from '@tanstack/react-router';
import {
  Building2,
  CalendarClock,
  CalendarDays,
  ChevronDown,
  Clock,
  FileText,
  LogOut,
  Moon,
  Settings2,
  Sun,
  Umbrella,
  Users,
} from 'lucide-react';
import { Button } from '#/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '#/components/ui/dropdown-menu';
import { useTheme } from '#/hooks/use-theme';
import { signOut } from '#/lib/auth-client';
import { cn } from '#/lib/utils';

type NavItem = {
  label: string;
  to: string;
  icon: React.ComponentType<{ className?: string }>;
};

const primaryNavItems: NavItem[] = [
  { label: 'Time entry', to: '/time-entry', icon: Clock },
  { label: 'Timesheets', to: '/timesheets', icon: CalendarDays },
  { label: 'Leave overview', to: '/leave-overview', icon: Umbrella },
];

const adminNavItems: NavItem[] = [
  { label: 'Customers', to: '/admin/customers', icon: Building2 },
  { label: 'Users', to: '/admin/users', icon: Users },
  { label: 'Contracts', to: '/admin/contracts', icon: FileText },
  { label: 'Leave types', to: '/admin/leave-types', icon: CalendarClock },
];

function EuricomMark({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 256 256" fill="none" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">
      <path
        d="M39.981 39.9826H109.998V0H35.1838C15.7526 0 0 15.7532 0 35.1852V110.003H39.9675V39.9826H39.981Z"
        fill="#00FF00"
      />
      <path
        d="M156.702 128.008C156.702 143.856 143.86 156.711 128 156.711C112.14 156.711 99.2979 143.869 99.2979 128.008C99.2979 112.147 112.153 99.3047 128 99.3047C143.847 99.3047 156.702 112.147 156.702 128.008Z"
        fill="#00FF00"
      />
      <path
        d="M220.809 0H145.994V39.9691H216.011V109.989H255.979V35.1852C255.979 15.7532 240.226 0 220.795 0"
        fill="#00FF00"
      />
      <path
        d="M216.019 145.998V216.018H146.002V255.987H220.816C240.248 255.987 256 240.234 256 220.802V145.984H216.033L216.019 145.998Z"
        fill="#00FF00"
      />
      <path
        d="M39.9832 216.016V145.996H0.015625V220.814C0.015625 240.246 15.7681 255.999 35.1994 255.999H110.014V216.03H39.9966L39.9832 216.016Z"
        fill="#00FF00"
      />
    </svg>
  );
}

function NavLink({ item, isActive }: { item: NavItem; isActive: boolean }) {
  return (
    <li className="flex h-full items-center">
      <Link
        to={item.to}
        className={cn(
          'relative flex h-full items-center gap-2.5 px-3.5 text-sm font-medium tracking-[0.01em] whitespace-nowrap transition-colors duration-150',
          isActive ? 'font-bold text-[#014046] dark:text-[#00FF00]' : 'text-muted-foreground hover:text-foreground',
        )}
      >
        <item.icon className="h-4 w-4 opacity-85 shrink-0" />
        {item.label}
      </Link>
    </li>
  );
}

function getInitials(name: string | null | undefined): string {
  if (!name) return '?';
  return name
    .split(' ')
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0].toUpperCase())
    .join('');
}

export function AppNavbar() {
  const { isDark, toggle } = useTheme();
  const pathname = useLocation({ select: (state) => state.pathname });
  const isActive = (to: string) => pathname === to || pathname.startsWith(to + '/');
  const { currentUser } = useRouteContext({ from: '/_authed' });
  const isAdmin = currentUser?.role === 'Admin';

  return (
    <nav
      className="sticky top-0 z-10 flex h-16 shrink-0 items-center gap-5 border-b bg-background px-6 transition-colors duration-200"
      aria-label="Primary"
    >
      {/* Brand */}
      <Link
        to="/"
        className="flex h-full shrink-0 items-center gap-3 border-r border-border pr-5 text-foreground no-underline hover:no-underline"
        aria-label="Timesheet Zone home"
      >
        <EuricomMark className="h-7 w-7 shrink-0" />
        <span className="hidden text-[18px] font-bold tracking-[-0.005em] text-[#014046] dark:text-[#00FF00] sm:block whitespace-nowrap">
          Timesheet Zone
        </span>
      </Link>

      {/* Primary nav */}
      <ul className="flex h-full items-center gap-1 list-none m-0 p-0">
        {primaryNavItems.map((item) => (
          <NavLink key={item.to} item={item} isActive={isActive(item.to)} />
        ))}
      </ul>

      {/* Admin: dropdown on smaller screens, individual links on wide screens */}
      {isAdmin && (
        <>
          <div className="h-5 w-px bg-border" />

          {/* Wide: individual links */}
          <ul className="hidden h-full items-center gap-1 list-none m-0 p-0 2xl:flex">
            {adminNavItems.map((item) => (
              <NavLink key={item.to} item={item} isActive={isActive(item.to)} />
            ))}
          </ul>

          {/* Narrow: dropdown */}
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <button
                type="button"
                className={cn(
                  '2xl:hidden',
                  'flex h-full items-center gap-2 px-3.5 text-sm font-medium tracking-[0.01em] whitespace-nowrap transition-colors duration-150 outline-none',
                  adminNavItems.some((item) => isActive(item.to))
                    ? 'font-bold text-[#014046] dark:text-[#00FF00]'
                    : 'text-muted-foreground hover:text-foreground',
                )}
              >
                <Settings2 className="h-4 w-4 opacity-85 shrink-0" />
                Admin
                <ChevronDown className="h-3.5 w-3.5 opacity-60" />
              </button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="start" className="w-44">
              {adminNavItems.map((item) => (
                <DropdownMenuItem key={item.to} asChild>
                  <Link
                    to={item.to}
                    className={cn(
                      'flex items-center gap-2.5 cursor-pointer',
                      isActive(item.to) && 'font-semibold text-[#014046] dark:text-[#00FF00]',
                    )}
                  >
                    <item.icon className="h-4 w-4 shrink-0 opacity-70" />
                    {item.label}
                  </Link>
                </DropdownMenuItem>
              ))}
            </DropdownMenuContent>
          </DropdownMenu>
        </>
      )}

      <div className="flex-1" />

      {/* Actions */}
      <div className="flex shrink-0 items-center gap-3 h-full">
        <Button
          variant="ghost"
          size="icon"
          onClick={toggle}
          aria-label="Toggle theme"
          className="rounded-full border border-border hover:border-[#014046] hover:text-[#014046] hover:bg-[#014046]/5 dark:hover:border-[#00FF00] dark:hover:text-[#00FF00] dark:hover:bg-[#00FF00]/8"
        >
          {isDark ? <Sun className="h-[18px] w-[18px]" /> : <Moon className="h-[18px] w-[18px]" />}
        </Button>

        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <button
              type="button"
              className="flex items-center gap-2.5 rounded-full border border-border/40 bg-transparent px-3 py-1.5 transition-colors duration-150 hover:border-border hover:bg-accent cursor-pointer outline-none"
              aria-label="Account menu"
            >
              <span
                className="flex h-7 w-7 shrink-0 items-center justify-center rounded-full bg-gradient-to-br from-[#00FF00] to-[#00C246] text-[11px] font-bold text-[#1D252D] leading-none"
                aria-hidden="true"
              >
                {getInitials(currentUser?.name)}
              </span>
              <span className="hidden text-sm font-semibold text-foreground lg:block whitespace-nowrap">
                {currentUser?.name ?? ''}
              </span>
              <ChevronDown className="h-3 w-3 text-muted-foreground" />
            </button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end" className="w-56">
            <div className="px-2 py-1.5">
              <p className="text-sm font-semibold">{currentUser?.name ?? ''}</p>
              <p className="text-xs text-muted-foreground">{currentUser?.email ?? ''}</p>
            </div>
            <DropdownMenuSeparator />
            <DropdownMenuItem
              className="cursor-pointer text-destructive focus:text-destructive"
              onSelect={() => void signOut('/login')}
            >
              <LogOut className="mr-2 h-4 w-4" />
              Sign out
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      </div>
    </nav>
  );
}
