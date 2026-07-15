import { RKind } from './r-kind';

export interface RDescriptor {
  /** The R name, e.g. `Render`. */
  readonly name: RKind;
  /** The poetic movement title from the mocks, e.g. `Build toward what you saw`. */
  readonly poeticName: string;
  /** A one-line description of the movement. */
  readonly desc: string;
}

/**
 * The five movements of the 5R co-creation loop, with the poetic titles and copy
 * taken verbatim from the mocks (docs/mocks/5r-loop.html).
 */
export const R_DESCRIPTORS: Record<RKind, RDescriptor> = {
  Request: {
    name: 'Request',
    poeticName: 'Invite the Spirit into the work',
    desc: "Name what you're seeking before you start. A short, honest ask.",
  },
  Receive: {
    name: 'Receive',
    poeticName: 'Wait, and write down what comes',
    desc: 'Capture the ideas, nudges, and constraints that surface — without editing them yet.',
  },
  Review: {
    name: 'Review',
    poeticName: 'Synthesize what emerged',
    desc: 'Shape the raw ideas into a direction the team can build.',
  },
  Render: {
    name: 'Render',
    poeticName: 'Build toward what you saw',
    desc: 'Make it real. Link the work, note what changed from the vision, and log the movement to continue.',
  },
  Rejoice: {
    name: 'Rejoice',
    poeticName: 'Give thanks for what was made',
    desc: 'Name the good in what came about — and who it serves. Rejoice closes the loop.',
  },
};

export function describeR(kind: RKind): RDescriptor {
  return R_DESCRIPTORS[kind];
}
