// Colour palette using full static Tailwind class strings so JIT picks them up.
// Each entry contains the three usage sites:
//   dot          — filled circle in BalanceRow and future chips
//   outline      — border colour for calendar day outlines in S11.4
//   focusOutline — focus-visible ring colour for calendar day cells in S11.4
const palette: Array<{ dot: string; outline: string; focusOutline: string }> = [
  { dot: 'bg-green-500', outline: 'border-green-500', focusOutline: 'focus-visible:ring-green-500' },
  { dot: 'bg-orange-500', outline: 'border-orange-500', focusOutline: 'focus-visible:ring-orange-500' },
  { dot: 'bg-blue-500', outline: 'border-blue-500', focusOutline: 'focus-visible:ring-blue-500' },
  { dot: 'bg-purple-500', outline: 'border-purple-500', focusOutline: 'focus-visible:ring-purple-500' },
  { dot: 'bg-pink-500', outline: 'border-pink-500', focusOutline: 'focus-visible:ring-pink-500' },
  { dot: 'bg-yellow-500', outline: 'border-yellow-500', focusOutline: 'focus-visible:ring-yellow-500' },
  { dot: 'bg-cyan-500', outline: 'border-cyan-500', focusOutline: 'focus-visible:ring-cyan-500' },
];

/**
 * Maps a leave-type name to a deterministic colour entry from the palette.
 *
 * Hashing is case-sensitive (matches the getAvatarColor convention in utils.ts).
 * Renaming a leave type intentionally shifts its colour — no schema field stores colour.
 */
export function getLeaveTypeColor(name: string): { dot: string; outline: string; focusOutline: string } {
  const hash = [...name].reduce((accumulator, character) => accumulator + character.charCodeAt(0), 0);
  return palette[hash % palette.length];
}
