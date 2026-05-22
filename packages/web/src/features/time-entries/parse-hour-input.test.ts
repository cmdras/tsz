import { describe, expect, it } from 'vitest';
import { parseHourInput } from './parse-hour-input';

describe('parseHourInput', () => {
  it('returns null for empty string', () => {
    expect(parseHourInput('')).toBeNull();
  });

  it('returns null for whitespace-only string', () => {
    expect(parseHourInput('  ')).toBeNull();
  });

  it('returns null for non-numeric input', () => {
    expect(parseHourInput('abc')).toBeNull();
  });

  describe('comma normalization', () => {
    it('accepts comma as decimal separator', () => {
      expect(parseHourInput('3,5')).toBe(3.5);
    });

    it('normalizes comma to period for whole numbers', () => {
      expect(parseHourInput('4,0')).toBe(4);
    });
  });

  describe('grain rejection', () => {
    it('rejects 0.25 (not a multiple of 0.5)', () => {
      expect(parseHourInput('0.25')).toBeNull();
    });

    it('rejects 1.3', () => {
      expect(parseHourInput('1.3')).toBeNull();
    });

    it('rejects 0.1', () => {
      expect(parseHourInput('0.1')).toBeNull();
    });
  });

  describe('range rejection', () => {
    it('rejects negative values', () => {
      expect(parseHourInput('-1')).toBeNull();
    });

    it('rejects values above 24', () => {
      expect(parseHourInput('25')).toBeNull();
    });

    it('rejects 24.5', () => {
      expect(parseHourInput('24.5')).toBeNull();
    });
  });

  describe('valid inputs', () => {
    it('accepts 0', () => {
      expect(parseHourInput('0')).toBe(0);
    });

    it('accepts 0.5', () => {
      expect(parseHourInput('0.5')).toBe(0.5);
    });

    it('accepts 4', () => {
      expect(parseHourInput('4')).toBe(4);
    });

    it('accepts 7.5', () => {
      expect(parseHourInput('7.5')).toBe(7.5);
    });

    it('accepts 24 (upper boundary)', () => {
      expect(parseHourInput('24')).toBe(24);
    });

    it('trims whitespace before parsing', () => {
      expect(parseHourInput('  8  ')).toBe(8);
    });
  });
});
