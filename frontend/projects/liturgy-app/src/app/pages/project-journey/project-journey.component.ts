import { ChangeDetectionStrategy, Component, DestroyRef, OnDestroy, OnInit, effect, inject, input, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { BoardRealtime, GatesService, ProjectJourney, ProjectsService } from '@liturgy/api';
import { GateComponent, RequirementToggle } from '@liturgy/components';

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

  readonly id = input.required<string>();
  readonly journey = signal<ProjectJourney | null>(null);

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
    this.projectsService.get(id).subscribe((journey) => this.journey.set(journey));
  }

  private reload(): void {
    this.load(this.id());
  }
}
