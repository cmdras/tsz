import { createFileRoute, redirect } from '@tanstack/react-router';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '#/components/ui/card';
import { Button } from '#/components/ui/button';
import { getSessionServerFn } from '#/lib/session';
import { signInWithMicrosoft } from '#/lib/auth-client';

export const Route = createFileRoute('/login')({
  beforeLoad: async () => {
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
