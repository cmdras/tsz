import { describe, it, expect } from 'vite-plus/test';
import { updateAnimalSchema, saveAnimalSchema } from './schemas';

describe('updateAnimalSchema', () => {
  it('accepts a valid animal', () => {
    const result = updateAnimalSchema.safeParse({ name: 'Rex', species: 'dog', age: 3 });
    expect(result.success).toBe(true);
  });

  it('accepts age = 0', () => {
    const result = updateAnimalSchema.safeParse({ name: 'Rex', species: 'dog', age: 0 });
    expect(result.success).toBe(true);
  });

  it('trims and rejects whitespace-only name', () => {
    const result = updateAnimalSchema.safeParse({ name: '   ', species: 'dog', age: 3 });
    expect(result.success).toBe(false);
    expect(result.error?.issues[0]?.message).toBe('Name is required');
    expect(result.error?.issues[0]?.path).toEqual(['name']);
  });

  it('rejects empty name with message', () => {
    const result = updateAnimalSchema.safeParse({ name: '', species: 'dog', age: 3 });
    expect(result.success).toBe(false);
    expect(result.error?.issues[0]?.message).toBe('Name is required');
    expect(result.error?.issues[0]?.path).toEqual(['name']);
  });

  it('rejects empty species with message', () => {
    const result = updateAnimalSchema.safeParse({ name: 'Rex', species: '', age: 3 });
    expect(result.success).toBe(false);
    expect(result.error?.issues[0]?.message).toBe('Species is required');
    expect(result.error?.issues[0]?.path).toEqual(['species']);
  });

  it('rejects negative age with message', () => {
    const result = updateAnimalSchema.safeParse({ name: 'Rex', species: 'dog', age: -1 });
    expect(result.success).toBe(false);
    expect(result.error?.issues[0]?.message).toBe('Age must be 0 or greater');
    expect(result.error?.issues[0]?.path).toEqual(['age']);
  });

  it('rejects non-integer age', () => {
    const result = updateAnimalSchema.safeParse({ name: 'Rex', species: 'dog', age: 3.5 });
    expect(result.success).toBe(false);
    expect(result.error?.issues[0]?.path).toEqual(['age']);
  });

  it('rejects non-number age', () => {
    const result = updateAnimalSchema.safeParse({ name: 'Rex', species: 'dog', age: '3' });
    expect(result.success).toBe(false);
    expect(result.error?.issues[0]?.path).toEqual(['age']);
  });

  it('rejects missing fields', () => {
    const result = updateAnimalSchema.safeParse({});
    expect(result.success).toBe(false);
    const paths = result.error?.issues.map((i) => i.path[0]).sort();
    expect(paths).toEqual(['age', 'name', 'species']);
  });
});

describe('saveAnimalSchema', () => {
  it('accepts a valid payload', () => {
    const result = saveAnimalSchema.safeParse({
      id: 7,
      animal: { name: 'Rex', species: 'dog', age: 3 },
    });
    expect(result.success).toBe(true);
  });

  it('rejects non-integer id', () => {
    const result = saveAnimalSchema.safeParse({
      id: 1.5,
      animal: { name: 'Rex', species: 'dog', age: 3 },
    });
    expect(result.success).toBe(false);
    expect(result.error?.issues[0]?.path).toEqual(['id']);
  });

  it('surfaces nested animal errors', () => {
    const result = saveAnimalSchema.safeParse({
      id: 1,
      animal: { name: '', species: 'dog', age: 3 },
    });
    expect(result.success).toBe(false);
    expect(result.error?.issues[0]?.path).toEqual(['animal', 'name']);
  });
});
