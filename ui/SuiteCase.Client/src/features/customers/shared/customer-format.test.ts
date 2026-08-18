import { describe, expect, it } from 'vitest';
import { formatDate } from './customer-format';

describe('formatDate', () => {
  it('formats ISO dates with English month abbreviations', () => {
    expect(formatDate('2026-02-10')).toBe('10 Feb 2026');
    expect(formatDate('1986-09-21')).toBe('21 Sept 1986');
  });

  it('preserves missing and unsupported values', () => {
    expect(formatDate(null)).toBe('Not provided');
    expect(formatDate('not-a-date')).toBe('not-a-date');
    expect(formatDate('2026-13-10')).toBe('2026-13-10');
  });
});
