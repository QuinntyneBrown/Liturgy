import { Injectable, inject, signal } from '@angular/core';
import { ProjectJourney, ProjectsService } from '@liturgy/api';

/**
 * Drives the shell's rail. Pages declare their context: workspace pages call
 * `showWorkspace()`; project pages call `showProject(id)` (the service fetches that
 * project's 4D journey for the Rhythm Rail). The Develop/loop pages additionally set
 * the live 5R count so the rail can nest the dial/list.
 */
@Injectable({ providedIn: 'root' })
export class RailContextService {
  private readonly projects = inject(ProjectsService);

  private readonly _mode = signal<'workspace' | 'project'>('workspace');
  private readonly _journey = signal<ProjectJourney | null>(null);
  private readonly _fiveRFilled = signal<number | null>(null);
  private currentProjectId: string | null = null;

  readonly mode = this._mode.asReadonly();
  readonly journey = this._journey.asReadonly();
  readonly fiveRFilled = this._fiveRFilled.asReadonly();

  showWorkspace(): void {
    this._mode.set('workspace');
    this._journey.set(null);
    this._fiveRFilled.set(null);
    this.currentProjectId = null;
  }

  showProject(projectId: string, fiveRFilled: number | null = null): void {
    this._mode.set('project');
    this._fiveRFilled.set(fiveRFilled);
    if (this.currentProjectId !== projectId) {
      this.currentProjectId = projectId;
      this._journey.set(null);
      this.projects.get(projectId).subscribe((journey) => this._journey.set(journey));
    }
  }

  /** Used by pages that already hold the journey, to avoid a second fetch. */
  setProjectJourney(journey: ProjectJourney, fiveRFilled: number | null = null): void {
    this._mode.set('project');
    this.currentProjectId = journey.id;
    this._journey.set(journey);
    this._fiveRFilled.set(fiveRFilled);
  }

  setFiveR(filled: number | null): void {
    this._fiveRFilled.set(filled);
  }

  /** Re-fetch the current project's journey (e.g. after a gate toggle unlocks a phase). */
  refresh(): void {
    if (this.currentProjectId) {
      this.projects.get(this.currentProjectId).subscribe((journey) => this._journey.set(journey));
    }
  }
}
