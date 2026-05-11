import { Link } from '@tanstack/react-router';
import { Moon, Sun } from 'lucide-react';
import { Button } from '#/components/ui/button';
import { useTheme } from '#/hooks/use-theme';

export function AppHeader() {
  const { isDark, toggle } = useTheme();

  return (
    <header className="flex h-14 shrink-0 items-center gap-2 border-b px-4">
      <Link to="/" className="font-semibold text-foreground hover:text-foreground/80">
        Timesheet Zone
      </Link>
      <Button variant="ghost" size="icon-sm" onClick={toggle} className="ml-auto" aria-label="Toggle theme">
        {isDark ? <Sun /> : <Moon />}
      </Button>
    </header>
  );
}
