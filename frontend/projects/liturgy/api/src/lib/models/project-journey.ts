import { Phase } from './phase';
import { PhaseKind } from './phase-kind';

export interface ProjectJourney {
  id: string;
  name: string;
  tag: string;
  currentPhase: PhaseKind;
  phases: Phase[];
}
