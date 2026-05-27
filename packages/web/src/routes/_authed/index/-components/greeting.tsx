interface GreetingAccent {
  text: string;
  className: string;
}

interface GreetingProps {
  name: string;
  loadTime: Date;
  accent: GreetingAccent;
}

const dutchWeekdayMonthFormatter = new Intl.DateTimeFormat('nl-NL', {
  weekday: 'long',
  day: 'numeric',
  month: 'long',
  year: 'numeric',
});

const timeFormatter = new Intl.DateTimeFormat('nl-NL', {
  hour: '2-digit',
  minute: '2-digit',
  hour12: false,
});

function formatLoadTimestamp(date: Date): string {
  const datePart = dutchWeekdayMonthFormatter.format(date);
  const timePart = timeFormatter.format(date);
  // Capitalise the first letter (weekday is lowercase in nl-NL)
  const capitalisedDate = datePart.charAt(0).toUpperCase() + datePart.slice(1);
  return `${capitalisedDate} · ${timePart}`;
}

export function Greeting({ name, loadTime, accent }: GreetingProps) {
  const timestamp = formatLoadTimestamp(loadTime);

  return (
    <div className="flex flex-col items-center gap-2 text-center">
      <p className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">{timestamp}</p>
      <h1 className="text-4xl font-bold tracking-tight">
        Hi {name}, <em className={`not-italic ${accent.className}`}>{accent.text}</em>
      </h1>
    </div>
  );
}
