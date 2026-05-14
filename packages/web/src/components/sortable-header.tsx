import { ArrowDown, ArrowUp, ArrowUpDown } from 'lucide-react';
import { TableHead } from '#/components/ui/table';

interface SortableHeaderProps {
  column: string;
  label: string;
  active: string | undefined;
  onToggle: (column: string) => void;
}

export function SortableHeader({ column, label, active, onToggle }: SortableHeaderProps) {
  const isAsc = active === column;
  const isDesc = active === `${column}-`;
  const Icon = isAsc ? ArrowUp : isDesc ? ArrowDown : ArrowUpDown;
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
