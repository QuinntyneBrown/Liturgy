import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ProjectSummary, ProjectsService } from '@liturgy/api';

@Component({
  selector: 'lit-projects',
  imports: [RouterLink],
  templateUrl: './projects.component.html',
  styleUrl: './projects.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProjectsComponent {
  private readonly projectsService = inject(ProjectsService);

  readonly projects = signal<ProjectSummary[]>([]);
  readonly loading = signal(true);

  constructor() {
    this.projectsService.list().subscribe({
      next: (projects) => {
        this.projects.set(projects);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  phaseBadge(phase: string): string {
    return `badge badge--${phase.toLowerCase()}`;
  }
}
