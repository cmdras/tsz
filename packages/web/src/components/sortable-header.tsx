import { ArrowDown, ArrowUp, ArrowUpDown } from 'lucide-react';
import { TableHead } from '#/components/ui/table';

export type SortDir = 'Asc' | 'Desc';

interface SortableHeaderProps<T extends string> {
  column: T;
  label: string;
  active: T;
  dir: SortDir;
  onToggle: (column: T) => void;
}

export function SortableHeader<T extends string>({ column, label, active, dir, onToggle }: SortableHeaderProps<T>) {
  const isActive = active === column;
  const Icon = isActive ? (dir === 'Asc' ? ArrowUp : ArrowDown) : ArrowUpDown;
  return (
    <TableHead>
      <button
        type="button"
        onClick={() => onToggle(column)}
        className="inline-flex items-center gap-1 font-medium hover:text-foreground"
      >
        {label}
        <Icon className="size-3.5" />
      </button>
    </TableHead>
  );
}
