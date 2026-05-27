interface GreetingAccent {
  text: string;
  className: string;
}

interface GreetingProps {
  name: string;
  accent: GreetingAccent;
}

export function Greeting({ name, accent }: GreetingProps) {
  return (
    <div className="flex flex-col items-center gap-2 text-center">
      <h1 className="text-4xl font-bold tracking-tight">
        Hi {name}, <em className={`not-italic ${accent.className}`}>{accent.text}</em>
      </h1>
    </div>
  );
}
