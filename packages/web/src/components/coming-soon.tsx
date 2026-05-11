import { Card, CardContent, CardHeader, CardTitle } from '#/components/ui/card';

interface ComingSoonProps {
  slice: string;
  description?: string;
}

export function ComingSoon({ slice, description }: ComingSoonProps) {
  return (
    <Card className="max-w-md">
      <CardHeader>
        <CardTitle>Coming in {slice}</CardTitle>
      </CardHeader>
      {description && <CardContent>{description}</CardContent>}
    </Card>
  );
}
