import { HeadContent, Link, Outlet, Scripts, createRootRoute } from '@tanstack/react-router';
import { Moon, Sun } from 'lucide-react';

import appCss from '../styles.css?url';
import { Button } from '#/components/ui/button';
import { ErrorBoundary } from '#/components/error-boundary';
import { useTheme } from '#/hooks/use-theme';

export const Route = createRootRoute({
  head: () => ({
    meta: [
      { charSet: 'utf-8' },
      { name: 'viewport', content: 'width=device-width, initial-scale=1' },
      { title: 'TanStack Start' },
    ],
    links: [{ rel: 'stylesheet', href: appCss }],
  }),
  component: RootLayout,
  shellComponent: RootDocument,
  errorComponent: ErrorBoundary,
  notFoundComponent: () => (
    <main>
      <h1 className="text-2xl font-bold">Page not found</h1>
      <p className="mt-2 text-gray-600">The page you're looking for doesn't exist.</p>
    </main>
  ),
});

const themeScript = `(function(){try{var t=localStorage.getItem('theme');if(t==='dark'||(!t&&window.matchMedia('(prefers-color-scheme: dark)').matches)){document.documentElement.classList.add('dark')}}catch(e){}})()`;

function RootDocument({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en" suppressHydrationWarning>
      <head>
        <script dangerouslySetInnerHTML={{ __html: themeScript }} />
        <HeadContent />
      </head>
      <body>
        {children}
        <Scripts />
      </body>
    </html>
  );
}

function RootLayout() {
  const { isDark, toggle } = useTheme();

  return (
    <div className="mx-auto max-w-3xl p-6">
      <nav className="mb-6 flex items-center gap-4 text-sm">
        <Link to="/" className="[&.active]:font-bold">
          Home
        </Link>
        <Link to="/animals" className="[&.active]:font-bold">
          Animals
        </Link>
        <Button variant="ghost" size="icon-sm" onClick={toggle} className="ml-auto" aria-label="Toggle theme">
          {isDark ? <Sun /> : <Moon />}
        </Button>
      </nav>
      <Outlet />
    </div>
  );
}
