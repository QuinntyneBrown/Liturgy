import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import {
  AuthStateService,
  Dashboard,
  PHASE_ORDER,
  PhaseKind,
  ProjectLane,
  ProjectsService,
  DashboardService,
  phaseBadgeClass as badgeClassFor,
  phaseOrdinal as ordinalFor,
} from '@liturgy/api';
import { RailContextService } from '../../shell/rail-context.service';

/** A 4D board column: a phase and the lanes' worth of projects currently in it. */
interface Lane {
  readonly phase: PhaseKind;
  readonly projects: ProjectLane[];
}

@Component({
  selector: 'lit-dashboard',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardComponent implements OnInit {
  private readonly dashboardService = inject(DashboardService);
  private readonly projectsService = inject(ProjectsService);
  private readonly authState = inject(AuthStateService);
  private readonly rail = inject(RailContextService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  readonly dashboard = signal<Dashboard | null>(null);
  readonly showNewProject = signal(false);
  readonly creating = signal(false);

  readonly newProjectForm = this.fb.nonNullable.group({
    name: ['', [Validators.required]],
    tag: ['', [Validators.required]],
  });

  /** "Good morning, Quinn." once the session is known, else the plain greeting. */
  readonly greeting = computed(() => {
    const firstName = this.authState.user()?.firstName;
    return firstName ? `Good morning, ${firstName}.` : 'Good morning.';
  });

  /** The 4D board's four lanes, in cycle order, each holding its projects. */
  readonly lanes = computed<Lane[]>(() => {
    const projects = this.dashboard()?.projects ?? [];
    return PHASE_ORDER.map((phase) => ({
      phase,
      projects: projects.filter((p) => p.currentPhase === phase),
    }));
  });

  constructor() {
    this.dashboardService.get().subscribe((dashboard) => this.dashboard.set(dashboard));
  }

  ngOnInit(): void {
    this.rail.showWorkspace();
  }

  phaseBadgeClass(phase: PhaseKind): string {
    return badgeClassFor(phase);
  }

  phaseOrdinal(phase: PhaseKind): string {
    return ordinalFor(phase);
  }

  toggleNewProject(): void {
    this.showNewProject.update((open) => !open);
  }

  createProject(): void {
    if (this.newProjectForm.invalid || this.creating()) {
      this.newProjectForm.markAllAsTouched();
      return;
    }

    this.creating.set(true);
    const { name, tag } = this.newProjectForm.getRawValue();
    this.projectsService.create(name, tag).subscribe({
      next: (created) => void this.router.navigate(['/projects', created.id]),
      error: () => this.creating.set(false),
    });
  }
}
