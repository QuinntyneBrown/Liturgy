import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { Gate, Requirement } from '@liturgy/api';

export interface RequirementToggle {
  requirement: Requirement;
  done: boolean;
}

/**
 * The enforcement gate: a titled bar with an open/blocked lock, and a checklist of
 * requirements. Toggling a requirement emits upward; the server recomputes gate state.
 */
@Component({
  selector: 'lit-gate',
  templateUrl: './gate.component.html',
  styleUrl: './gate.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class GateComponent {
  readonly gate = input.required<Gate>();
  readonly toggle = output<RequirementToggle>();

  readonly remaining = computed(
    () => this.gate().requirements.filter((r) => r.state !== 'Done').length,
  );

  onToggle(requirement: Requirement): void {
    this.toggle.emit({ requirement, done: requirement.state !== 'Done' });
  }
}
