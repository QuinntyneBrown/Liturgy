import { PhaseKind } from './phase-kind';

/** Momentum stats across the workspace. */
export interface Momentum {
  activeProjects: number;
  movementsThisWeek: number;
  gatesBlocked: number;
  weeksCompounded: number;
}

/** A gate or card that needs attention (surfaced enforcement). */
export interface Attention {
  projectId: string;
  projectName: string;
  title: string;
  reason: string;
  /** `'gate'` links to the project's gate/journey; `'loop'` links to the 5R loop. */
  actionKind: 'gate' | 'loop';
  actionTargetId: string;
}

/** A project positioned on the 4D board. */
export interface ProjectLane {
  id: string;
  name: string;
  currentPhase: PhaseKind;
  meta: string;
  blocked: boolean;
}

/** The workspace dashboard aggregate. */
export interface Dashboard {
  momentum: Momentum;
  attention: Attention[];
  projects: ProjectLane[];
}
