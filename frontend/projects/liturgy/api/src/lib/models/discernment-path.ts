/** The four discernment paths of the Discern phase. */
export type DiscernmentPath = 'Reject' | 'Receive' | 'Reimagine' | 'Create';

export interface DiscernmentPathDescriptor {
  readonly path: DiscernmentPath;
  readonly ordinal: string;
  readonly desc: string;
}

/** The four paths with the copy from docs/mocks/discern-gate.html, in display order. */
export const DISCERNMENT_PATHS: readonly DiscernmentPathDescriptor[] = [
  {
    path: 'Reject',
    ordinal: 'Path 01',
    desc: 'The most redemptive answer may be no technology at all. Name why, and close the loop with peace.',
  },
  {
    path: 'Receive',
    ordinal: 'Path 02',
    desc: 'A good solution already exists. Adopt it, support its makers, and point people to it.',
  },
  {
    path: 'Reimagine',
    ordinal: 'Path 03',
    desc: 'An existing intervention almost fits. Adapt it redemptively for the people you serve.',
  },
  {
    path: 'Create',
    ordinal: 'Path 04',
    desc: 'Nothing fits. Build something new — cautiously, guarding against misuse from the first line.',
  },
];
