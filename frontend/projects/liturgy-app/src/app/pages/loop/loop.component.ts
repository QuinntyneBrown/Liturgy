import { ChangeDetectionStrategy, Component, DestroyRef, computed, effect, inject, input, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { BoardRealtime, CardLoop, LogMovementRequest, LoopService, Movement, RKind } from '@liturgy/api';
import { DialComponent } from '@liturgy/components';

const R_DESCRIPTIONS: Record<RKind, { name: string; desc: string }> = {
  Request: { name: 'Request', desc: 'Invite the Spirit into the work. Name what you are seeking.' },
  Receive: { name: 'Receive', desc: 'Wait, and write down what comes — without editing.' },
  Review: { name: 'Review', desc: 'Synthesize what emerged into a buildable direction.' },
  Render: { name: 'Render', desc: 'Build toward what you saw. Link the artifact; note what changed.' },
  Rejoice: { name: 'Rejoice', desc: 'Give thanks for what was made, and name who it serves.' },
};

@Component({
  selector: 'lit-loop',
  imports: [ReactiveFormsModule, DialComponent],
  templateUrl: './loop.component.html',
  styleUrl: './loop.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoopComponent {
  private readonly loopService = inject(LoopService);
  private readonly realtime = inject(BoardRealtime);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  readonly cardId = input.required<string>();
  readonly loop = signal<CardLoop | null>(null);
  readonly submitting = signal(false);

  readonly currentMovement = computed(() => this.loop()?.movements.find((m) => m.state === 'Current') ?? null);

  readonly form = this.fb.nonNullable.group({
    ask: [''],
    received: [''],
    synthesis: [''],
    artifactUrl: [''],
    whatChanged: [''],
    thanksgiving: [''],
  });

  constructor() {
    this.realtime.movementLogged$.pipe(takeUntilDestroyed()).subscribe((loop) => {
      if (loop.cardId === this.cardId()) {
        this.loop.set(loop);
      }
    });

    effect(() => {
      const id = this.cardId();
      this.load(id);
    });

    this.destroyRef.onDestroy(() => void this.realtime.stop());
  }

  describe(kind: RKind): { name: string; desc: string } {
    return R_DESCRIPTIONS[kind];
  }

  content(movement: Movement): string {
    switch (movement.kind) {
      case 'Request':
        return movement.ask ?? '';
      case 'Receive':
        return movement.received ?? '';
      case 'Review':
        return movement.synthesis ?? '';
      case 'Render':
        return [movement.artifactUrl, movement.whatChanged].filter(Boolean).join(' — ');
      case 'Rejoice':
        return movement.thanksgiving ?? '';
    }
  }

  log(): void {
    const current = this.currentMovement();
    if (!current || this.submitting()) {
      return;
    }

    this.submitting.set(true);
    const raw = this.form.getRawValue();
    const request: LogMovementRequest = {
      kind: current.kind,
      ask: raw.ask || null,
      received: raw.received || null,
      synthesis: raw.synthesis || null,
      artifactUrl: raw.artifactUrl || null,
      whatChanged: raw.whatChanged || null,
      thanksgiving: raw.thanksgiving || null,
    };

    this.loopService.logMovement(this.cardId(), request).subscribe({
      next: (loop) => {
        this.loop.set(loop);
        this.form.reset();
        this.submitting.set(false);
      },
      error: () => this.submitting.set(false),
    });
  }

  markDone(): void {
    this.loopService.markDone(this.cardId()).subscribe({
      next: (card) => void this.router.navigate(['/board', card.projectId]),
    });
  }

  private load(id: string): void {
    this.loopService.get(id).subscribe((loop) => this.loop.set(loop));
  }
}
