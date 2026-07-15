// Traces to: L2-UX-002
import { phaseBadgeClass, phaseOrdinal, phaseSlug } from './phase-descriptor';

describe('phaseSlug', () => {
  it('maps Demonstrate to the "demo" slug the stylesheet actually defines', () => {
    // Regression: naive toLowerCase() produced `.badge--demonstrate`, which is unstyled.
    expect(phaseSlug('Demonstrate')).toBe('demo');
  });

  it('lowercases the other phases', () => {
    expect(phaseSlug('Discover')).toBe('discover');
    expect(phaseSlug('Discern')).toBe('discern');
    expect(phaseSlug('Develop')).toBe('develop');
  });
});

describe('phaseBadgeClass', () => {
  it('builds the badge class from the slug', () => {
    expect(phaseBadgeClass('Demonstrate')).toBe('badge badge--demo');
    expect(phaseBadgeClass('Develop')).toBe('badge badge--develop');
  });
});

describe('phaseOrdinal', () => {
  it('is the two-digit position in the 4D cycle', () => {
    expect(phaseOrdinal('Discover')).toBe('01');
    expect(phaseOrdinal('Demonstrate')).toBe('04');
  });
});
