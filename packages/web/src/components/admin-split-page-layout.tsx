import type { ReactNode } from 'react';

interface AdminSplitPageLayoutProps {
  title: string;
  newButton: ReactNode;
  list: ReactNode;
  detailKey?: string;
  children: ReactNode;
}

export function AdminSplitPageLayout({ title, newButton, list, detailKey, children }: AdminSplitPageLayoutProps) {
  return (
    <div className="flex flex-col gap-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">{title}</h1>
        {newButton}
      </div>
      <div className="flex gap-4 h-[calc(100vh-10rem)]">
        {list}
        <div key={detailKey ?? 'empty'} className="flex-1 border rounded-lg overflow-y-auto animate-fade-in">
          {children}
        </div>
      </div>
    </div>
  );
}
