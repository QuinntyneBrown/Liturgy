import { RKind } from './r-kind';

export interface LogMovementRequest {
  kind: RKind;
  ask?: string | null;
  received?: string | null;
  synthesis?: string | null;
  artifactUrl?: string | null;
  whatChanged?: string | null;
  thanksgiving?: string | null;
}
