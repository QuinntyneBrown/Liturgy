import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PhaseKind, ProjectSummary, ProjectsService, phaseBadgeClass } from '@liturgy/api';
import { RailContextService } from '../../shell/rail-context.service';

@Component({
  selector: 'lit-projects',
  imports: [RouterLink],
  templateUrl: './projects.component.html',
  styleUrl: './projects.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProjectsComponent {
  private readonly projectsService = inject(ProjectsService);
  private readonly rail = inject(RailContextService);

  readonly projects = signal<ProjectSummary[]>([]);
  readonly loading = signal(true);

  constructor() {
    this.rail.showWorkspace();
    this.projectsService.list().subscribe({
      next: (projects) => {
        this.projects.set(projects);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  phaseBadge(phase: PhaseKind): string {
    return phaseBadgeClass(phase);
  }
}
