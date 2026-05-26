import { describe, expect, it } from 'vitest';
import { getLeaveTypeColor } from './leave-type-color';

describe('getLeaveTypeColor', () => {
  describe('determinism', () => {
    it('returns the same colour for the same name on every call', () => {
      const firstCall = getLeaveTypeColor('Annual Leave');
      const secondCall = getLeaveTypeColor('Annual Leave');
      expect(firstCall).toEqual(secondCall);
    });

    it('returns the same colour object shape every time', () => {
      const result = getLeaveTypeColor('Sick Leave');
      expect(result).toEqual(getLeaveTypeColor('Sick Leave'));
    });
  });

  describe('palette index within bounds', () => {
    it('returns a valid palette entry for a typical name', () => {
      const result = getLeaveTypeColor('Annual Leave');
      expect(result).toHaveProperty('dot');
      expect(result).toHaveProperty('outline');
      expect(result).toHaveProperty('focusOutline');
    });

    it('returns non-empty string values for all three fields', () => {
      const result = getLeaveTypeColor('Parental Leave');
      expect(result.dot.length).toBeGreaterThan(0);
      expect(result.outline.length).toBeGreaterThan(0);
      expect(result.focusOutline.length).toBeGreaterThan(0);
    });

    it('dot field starts with bg- prefix (Tailwind background class)', () => {
      const result = getLeaveTypeColor('Special Leave');
      expect(result.dot).toMatch(/^bg-/);
    });

    it('outline field starts with border- prefix (Tailwind border class)', () => {
      const result = getLeaveTypeColor('Special Leave');
      expect(result.outline).toMatch(/^border-/);
    });

    it('focusOutline field starts with focus-visible:ring- prefix', () => {
      const result = getLeaveTypeColor('Special Leave');
      expect(result.focusOutline).toMatch(/^focus-visible:ring-/);
    });
  });

  describe('case sensitivity', () => {
    // Hashing is case-sensitive (consistent with getAvatarColor in utils.ts).
    // "annual leave" and "Annual Leave" differ in char codes and may map to different palette entries.
    it('treats the same name in different cases as different inputs', () => {
      const lower = getLeaveTypeColor('annual leave');
      const upper = getLeaveTypeColor('ANNUAL LEAVE');
      const mixed = getLeaveTypeColor('Annual Leave');
      // All are valid palette entries; lower and upper may differ from mixed
      expect(lower).toHaveProperty('dot');
      expect(upper).toHaveProperty('dot');
      expect(mixed).toHaveProperty('dot');
      // Assert they are not all identical (case-sensitivity is detectable across 3 variations)
      const allSame = lower.dot === upper.dot && upper.dot === mixed.dot;
      // This assertion documents intentional case-sensitive behaviour; if all three
      // hash to the same palette slot by coincidence this test should be updated with different names.
      expect(allSame).toBe(false);
    });
  });

  describe('empty string', () => {
    it('handles empty string without throwing', () => {
      expect(() => getLeaveTypeColor('')).not.toThrow();
    });

    it('returns a valid palette entry for empty string', () => {
      const result = getLeaveTypeColor('');
      expect(result).toHaveProperty('dot');
      expect(result).toHaveProperty('outline');
      expect(result).toHaveProperty('focusOutline');
    });

    it('empty string hash is 0, maps to first palette entry', () => {
      // sum of char codes of '' = 0, 0 % paletteSize = 0 → first entry (green)
      const result = getLeaveTypeColor('');
      expect(result.dot).toBe('bg-green-500');
    });
  });
});
