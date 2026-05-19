import { createFileRoute, redirect } from '@tanstack/react-router';
import { z } from 'zod';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '#/components/ui/card';
import { Button } from '#/components/ui/button';
import { getSessionServerFn } from '#/lib/session';
import { signInWithMicrosoft } from '#/lib/auth-client';

const searchSchema = z.object({
  error: z.string().optional(),
});

export const Route = createFileRoute('/login')({
  validateSearch: searchSchema,
  beforeLoad: async ({ search }) => {
    if (search.error) return;
    const session = await getSessionServerFn();
    if (session?.user) {
      console.log('[login] already signed in → /');
      throw redirect({ to: '/' });
    }
  },
  component: LoginPage,
});

function LoginPage() {
  const handleClick = () => {
    void signInWithMicrosoft('/');
  };

  return (
    <main className="grid min-h-screen place-items-center">
      <Card className="w-80">
        <CardHeader className="text-center">
          <CardTitle>Sign in</CardTitle>
          <CardDescription>Use your Euricom Microsoft account.</CardDescription>
        </CardHeader>
        <CardContent>
          <Button onClick={handleClick} className="w-full">
            Sign in with Microsoft
          </Button>
        </CardContent>
      </Card>
    </main>
  );
}
