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
  readonly includeClosed = signal(false);
  readonly editingId = signal<string | null>(null);
  readonly busyId = signal<string | null>(null);

  constructor() {
    this.rail.showWorkspace();
    this.reload();
  }

  private reload(): void {
    this.loading.set(true);
    this.projectsService.list(this.includeClosed()).subscribe({
      next: (projects) => {
        this.projects.set(projects);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  toggleClosed(): void {
    this.includeClosed.update((v) => !v);
    this.reload();
  }

  startEdit(project: ProjectSummary): void {
    this.editingId.set(project.id);
  }

  cancelEdit(): void {
    this.editingId.set(null);
  }

  saveEdit(project: ProjectSummary, name: string, tag: string): void {
    const trimmedName = name.trim();
    const trimmedTag = tag.trim();
    if (!trimmedName || !trimmedTag) {
      return;
    }
    this.busyId.set(project.id);
    this.projectsService.update(project.id, trimmedName, trimmedTag).subscribe({
      next: () => {
        this.editingId.set(null);
        this.busyId.set(null);
        this.reload();
      },
      error: () => this.busyId.set(null),
    });
  }

  close(project: ProjectSummary): void {
    this.busyId.set(project.id);
    this.projectsService.close(project.id).subscribe({
      next: () => {
        this.busyId.set(null);
        this.reload();
      },
      error: () => this.busyId.set(null),
    });
  }

  reopen(project: ProjectSummary): void {
    this.busyId.set(project.id);
    this.projectsService.reopen(project.id).subscribe({
      next: () => {
        this.busyId.set(null);
        this.reload();
      },
      error: () => this.busyId.set(null),
    });
  }

  remove(project: ProjectSummary): void {
    if (!confirm(`Delete "${project.name}"? This permanently removes the project and all its work.`)) {
      return;
    }
    this.busyId.set(project.id);
    this.projectsService.delete(project.id).subscribe({
      next: () => {
        this.busyId.set(null);
        this.reload();
      },
      error: () => this.busyId.set(null),
    });
  }

  phaseBadge(phase: PhaseKind): string {
    return phaseBadgeClass(phase);
  }
}
