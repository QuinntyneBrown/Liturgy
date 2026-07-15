import { DiscernmentPath } from './discernment-path';

/** A project's Discern decision: the chosen path plus the recorded rationale. */
export interface Decision {
  chosenPath: DiscernmentPath | null;
  rationale: string;
  prayedOverWith: string;
  decidedAt: string | null;
}

/** Payload for recording/updating a Discern decision. */
export interface SaveDecisionRequest {
  chosenPath: DiscernmentPath;
  rationale: string;
  prayedOverWith: string;
}
