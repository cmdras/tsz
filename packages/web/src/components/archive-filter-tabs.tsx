import { Tabs, TabsList, TabsTrigger } from '#/components/ui/tabs';
import { type ArchiveFilter } from '#/lib/archive-filter';

interface ArchiveFilterTabsProps {
  value?: ArchiveFilter;
  onValueChange: (value: ArchiveFilter) => void;
}

export function ArchiveFilterTabs({ value, onValueChange }: ArchiveFilterTabsProps) {
  return (
    <Tabs value={value ?? 'all'} onValueChange={(tabValue) => onValueChange(tabValue as ArchiveFilter)}>
      <TabsList className="w-full">
        <TabsTrigger value="all" className="flex-1">
          All
        </TabsTrigger>
        <TabsTrigger value="active" className="flex-1">
          Active
        </TabsTrigger>
        <TabsTrigger value="archived" className="flex-1">
          Archived
        </TabsTrigger>
      </TabsList>
    </Tabs>
  );
}
