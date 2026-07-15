import { ChangeDetectionStrategy, Component, computed, effect, inject, input, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import {
  DISCERNMENT_PATHS,
  Decision,
  DecisionService,
  DiscernmentPath,
  GatesService,
  ProjectJourney,
  ProjectsService,
} from '@liturgy/api';
import { GateComponent, RequirementToggle } from '@liturgy/components';
import { RailContextService } from '../../shell/rail-context.service';

@Component({
  selector: 'lit-discern',
  imports: [ReactiveFormsModule, GateComponent],
  templateUrl: './discern.component.html',
  styleUrl: './discern.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DiscernComponent {
  private readonly decisionService = inject(DecisionService);
  private readonly projectsService = inject(ProjectsService);
  private readonly gatesService = inject(GatesService);
  private readonly fb = inject(FormBuilder);
  private readonly rail = inject(RailContextService);

  readonly projectId = input.required<string>();

  /** The four discernment paths, in display order, for the choice cards. */
  readonly paths = DISCERNMENT_PATHS;

  readonly decision = signal<Decision | null>(null);
  readonly journey = signal<ProjectJourney | null>(null);
  readonly chosen = signal<DiscernmentPath | null>(null);
  readonly saving = signal(false);

  readonly form = this.fb.nonNullable.group({
    rationale: [''],
    prayedOverWith: [''],
  });

  readonly discernGate = computed(
    () => this.journey()?.phases.find((p) => p.kind === 'Discern')?.gate ?? null,
  );

  /** Develop route once the Discern gate is Open; null keeps the advance action locked. */
  readonly developLink = computed(() =>
    this.discernGate()?.state === 'Open' ? ['/board', this.projectId()] : null,
  );

  constructor() {
    effect(() => this.rail.showProject(this.projectId()));

    effect(() => {
      const id = this.projectId();
      this.load(id);
    });
  }

  choose(path: DiscernmentPath): void {
    this.chosen.set(path);
  }

  save(): void {
    const path = this.chosen();
    if (!path || this.saving()) {
      return;
    }

    this.saving.set(true);
    const raw = this.form.getRawValue();
    this.decisionService
      .save(this.projectId(), {
        chosenPath: path,
        rationale: raw.rationale,
        prayedOverWith: raw.prayedOverWith,
      })
      .subscribe({
        next: (decision) => {
          this.decision.set(decision);
          this.loadJourney();
          this.saving.set(false);
        },
        error: () => this.saving.set(false),
      });
  }

  onToggle(event: RequirementToggle): void {
    this.gatesService.toggleRequirement(event.requirement.id, event.done).subscribe({
      next: () => this.loadJourney(),
    });
  }

  private load(id: string): void {
    this.decisionService.get(id).subscribe((decision) => {
      this.decision.set(decision);
      this.chosen.set(decision.chosenPath);
      this.form.setValue({
        rationale: decision.rationale,
        prayedOverWith: decision.prayedOverWith,
      });
    });
    this.loadJourney();
  }

  private loadJourney(): void {
    this.projectsService.get(this.projectId()).subscribe((journey) => {
      this.journey.set(journey);
      // Keep the rail's gate-latch icons in sync now, without waiting for the
      // separate showProject() fetch to notice a projectId change that never comes.
      this.rail.setProjectJourney(journey);
    });
  }
}
