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
      <h1 className="text-2xl font-semibold">
        Hi {name}, <em className={`font-normal ${accent.className}`}>{accent.text}</em>
      </h1>
    </div>
  );
}
