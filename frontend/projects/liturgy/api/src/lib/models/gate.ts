import { GateState } from './gate-state';
import { Requirement } from './requirement';

export interface Gate {
  id: string;
  phaseId: string;
  title: string;
  state: GateState;
  requirements: Requirement[];
}
