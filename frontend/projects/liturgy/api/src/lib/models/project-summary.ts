import { PhaseKind } from './phase-kind';
import { ProjectStatus } from './project-status';

export interface ProjectSummary {
  id: string;
  name: string;
  tag: string;
  currentPhase: PhaseKind;
  status: ProjectStatus;
}
