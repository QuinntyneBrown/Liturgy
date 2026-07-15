import { PhaseKind } from './phase-kind';

export interface ProjectSummary {
  id: string;
  name: string;
  tag: string;
  currentPhase: PhaseKind;
}
