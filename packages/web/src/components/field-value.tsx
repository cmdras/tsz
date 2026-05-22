export function FieldValue({ label, value, className }: { label: string; value: string; className?: string }) {
  return (
    <div className={className}>
      <p className="text-[11px] font-medium uppercase tracking-[0.2em] text-muted-foreground">{label}</p>
      <p className="text-sm mt-0.5">{value || '—'}</p>
    </div>
  );
}
