import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Gate, Requirement } from '@liturgy/api';

export interface RequirementToggle {
  requirement: Requirement;
  done: boolean;
}

/**
 * The enforcement gate: a titled bar with an open/blocked lock, a checklist of
 * requirements, and an optional advance action. Toggling a requirement emits upward;
 * the server recomputes gate state. The advance control stays locked (padlock,
 * `aria-disabled`) until the gate is Open — the consistent locked-action language.
 */
@Component({
  selector: 'lit-gate',
  imports: [RouterLink],
  templateUrl: './gate.component.html',
  styleUrl: './gate.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GateComponent {
  readonly gate = input.required<Gate>();
  /** Label for the advance button, e.g. "Open Develop". When null, no advance control renders. */
  readonly advanceLabel = input<string | null>(null);
  /** Router link used once the gate is Open. */
  readonly advanceLink = input<unknown[] | null>(null);
  /** Helper text shown under the advance control. */
  readonly helperText = input<string | null>(null);
  readonly toggle = output<RequirementToggle>();

  readonly remaining = computed(
    () => this.gate().requirements.filter((r) => r.state !== 'Done').length,
  );
  readonly isOpen = computed(() => this.gate().state === 'Open');
  readonly remainingLabel = computed(() => {
    const n = this.remaining();
    return `${n} requirement${n === 1 ? '' : 's'} left`;
  });

  onToggle(requirement: Requirement): void {
    this.toggle.emit({ requirement, done: requirement.state !== 'Done' });
  }
}
