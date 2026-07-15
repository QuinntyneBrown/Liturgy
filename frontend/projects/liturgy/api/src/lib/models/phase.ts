import { Gate } from './gate';
import { PhaseKind } from './phase-kind';
import { PhaseState } from './phase-state';

export interface Phase {
  id: string;
  kind: PhaseKind;
  state: PhaseState;
  order: number;
  gate: Gate | null;
}
