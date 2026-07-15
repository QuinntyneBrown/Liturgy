import { PhaseKind } from './phase-kind';

/**
 * The 4D phases in cycle order.
 */
export const PHASE_ORDER: readonly PhaseKind[] = ['Discover', 'Discern', 'Develop', 'Demonstrate'];

/**
 * Maps a 4D phase to its short design-system slug used in BEM class names and badges.
 * Note the deliberate `Demonstrate → 'demo'`: the stylesheet defines `.badge--demo`,
 * `.badge--develop`, etc. — never `.badge--demonstrate` — so lowercasing the enum name
 * naively would leave the Demonstrate phase unstyled.
 */
export function phaseSlug(kind: PhaseKind): 'discover' | 'discern' | 'develop' | 'demo' {
  return kind === 'Demonstrate'
    ? 'demo'
    : (kind.toLowerCase() as 'discover' | 'discern' | 'develop');
}

/** The `badge badge--<slug>` class for a phase, e.g. `Demonstrate` → `badge badge--demo`. */
export function phaseBadgeClass(kind: PhaseKind): string {
  return `badge badge--${phaseSlug(kind)}`;
}

/** The two-digit ordinal for a phase, e.g. `Develop` → `03`. */
export function phaseOrdinal(kind: PhaseKind): string {
  return String(PHASE_ORDER.indexOf(kind) + 1).padStart(2, '0');
}
