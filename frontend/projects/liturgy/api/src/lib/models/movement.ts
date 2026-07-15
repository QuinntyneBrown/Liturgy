import { MovementState } from './movement-state';
import { RKind } from './r-kind';

export interface Movement {
  id: string;
  kind: RKind;
  order: number;
  state: MovementState;
  ask: string | null;
  received: string | null;
  synthesis: string | null;
  artifactUrl: string | null;
  whatChanged: string | null;
  thanksgiving: string | null;
  loggedAt: string | null;
}
