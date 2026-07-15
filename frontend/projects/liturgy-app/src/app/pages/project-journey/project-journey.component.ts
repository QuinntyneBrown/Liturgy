import { ChangeDetectionStrategy, Component, DestroyRef, OnDestroy, OnInit, effect, inject, input, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { BoardRealtime, GatesService, Phase, PhaseKind, ProjectJourney, ProjectsService, phaseBadgeClass } from '@liturgy/api';
import { GateComponent, RequirementToggle } from '@liturgy/components';
import { RailContextService } from '../../shell/rail-context.service';

@Component({
  selector: 'lit-project-journey',
  imports: [RouterLink, GateComponent],
  templateUrl: './project-journey.component.html',
  styleUrl: './project-journey.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProjectJourneyComponent implements OnInit, OnDestroy {
  private readonly projectsService = inject(ProjectsService);
  private readonly gatesService = inject(GatesService);
  private readonly realtime = inject(BoardRealtime);
  private readonly destroyRef = inject(DestroyRef);
  private readonly rail = inject(RailContextService);

  readonly id = input.required<string>();
  readonly journey = signal<ProjectJourney | null>(null);

  phaseBadge(kind: PhaseKind): string {
    return phaseBadgeClass(kind);
  }

  private screenFor(kind: PhaseKind, id: string): unknown[] {
    switch (kind) {
      case 'Discover':
        return ['/projects', id];
      case 'Discern':
        return ['/discern', id];
      case 'Develop':
        return ['/board', id];
      case 'Demonstrate':
        return ['/demonstrate', id];
    }
  }

  private nextKind(order: number): PhaseKind | null {
    return this.journey()?.phases.find((p) => p.order === order + 1)?.kind ?? null;
  }

  /** Label for a gate's advance action, e.g. "Advance to Develop". */
  advanceLabel(order: number): string | null {
    const next = this.nextKind(order);
    return next ? `Advance to ${next}` : null;
  }

  /** Advance route once the gate is Open; null keeps the action locked. */
  advanceLink(phase: Phase, id: string): unknown[] | null {
    const next = this.nextKind(phase.order);
    return phase.gate?.state === 'Open' && next ? this.screenFor(next, id) : null;
  }

  constructor() {
    // Reload whenever a collaborator changes a gate or unlocks a phase.
    this.realtime.gateChanged$.pipe(takeUntilDestroyed()).subscribe(() => this.reload());
    this.realtime.phaseUnlocked$.pipe(takeUntilDestroyed()).subscribe(() => this.reload());

    // React to route id changes (component is reused across project ids).
    effect(() => {
      const id = this.id();
      this.load(id);
      void this.realtime.start(id);
    });
  }

  ngOnInit(): void {
    // load handled by effect
  }

  ngOnDestroy(): void {
    void this.realtime.stop();
  }

  onToggle(event: RequirementToggle): void {
    this.gatesService.toggleRequirement(event.requirement.id, event.done).subscribe({
      next: () => this.reload(),
    });
  }

  private load(id: string): void {
    this.projectsService.get(id).subscribe((journey) => {
      this.journey.set(journey);
      this.rail.setProjectJourney(journey);
    });
  }

  private reload(): void {
    this.load(this.id());
  }
}
