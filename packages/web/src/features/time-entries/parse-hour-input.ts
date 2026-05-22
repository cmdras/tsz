export function parseHourInput(raw: string): number | null {
  const trimmed = raw.trim();
  if (trimmed === '') return null;

  const normalized = trimmed.replaceAll(',', '.');

  // Reject hex (0x8) and scientific notation (1e1) that Number() would silently accept
  if (normalized.includes('e') || normalized.includes('E') || normalized.includes('x') || normalized.includes('X'))
    return null;

  const value = Number(normalized);
  if (Number.isNaN(value)) return null;
  if (value < 0 || value > 24) return null;
  if ((value * 2) % 1 !== 0) return null; // must be a multiple of 0.5

  return value;
}
