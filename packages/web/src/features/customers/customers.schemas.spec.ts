import { describe, it, expect } from 'vite-plus/test';
import { customerSchema } from './customers.schemas';

const valid = {
  name: 'Alpha Corp',
  street: 'Keizerslaan 1',
  zip: '1000',
  city: 'Brussel',
  country: 'Belgium',
  contactName: 'Alice',
  contactEmail: 'alice@alpha.be',
};

describe('customerSchema', () => {
  it('accepts a valid customer', () => {
    expect(customerSchema.safeParse(valid).success).toBe(true);
  });

  it('accepts empty optional fields', () => {
    const result = customerSchema.safeParse({ ...valid, street: '', zip: '', contactName: '', contactEmail: '' });
    expect(result.success).toBe(true);
  });

  it('rejects empty name', () => {
    const result = customerSchema.safeParse({ ...valid, name: '' });
    expect(result.success).toBe(false);
    expect(result.error?.issues[0]?.path).toEqual(['name']);
    expect(result.error?.issues[0]?.message).toBe('Name is required');
  });

  it('rejects whitespace-only name', () => {
    const result = customerSchema.safeParse({ ...valid, name: '   ' });
    expect(result.success).toBe(false);
    expect(result.error?.issues[0]?.path).toEqual(['name']);
  });

  it('rejects empty country', () => {
    const result = customerSchema.safeParse({ ...valid, country: '' });
    expect(result.success).toBe(false);
    expect(result.error?.issues[0]?.path).toEqual(['country']);
    expect(result.error?.issues[0]?.message).toBe('Country is required');
  });

  it('rejects invalid email format', () => {
    const result = customerSchema.safeParse({ ...valid, contactEmail: 'not-an-email' });
    expect(result.success).toBe(false);
    expect(result.error?.issues[0]?.path).toEqual(['contactEmail']);
  });

  it('accepts empty string for contactEmail', () => {
    const result = customerSchema.safeParse({ ...valid, contactEmail: '' });
    expect(result.success).toBe(true);
  });

  it('rejects missing required fields', () => {
    const result = customerSchema.safeParse({});
    expect(result.success).toBe(false);
    const paths = result.error?.issues.map((i) => i.path[0]);
    expect(paths).toContain('name');
    expect(paths).toContain('country');
  });
});
