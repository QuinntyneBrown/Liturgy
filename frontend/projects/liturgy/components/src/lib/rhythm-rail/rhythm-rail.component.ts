import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Phase, PhaseKind, ProjectJourney, phaseOrdinal } from '@liturgy/api';
import { DialComponent } from '../dial/dial.component';
import { RListComponent } from '../r-list/r-list.component';

type LatchState = 'open' | 'blocked' | 'locked';

/**
 * The Rhythm Rail — the signature vertical 4D spine. Renders the four ordered
 * stations with their done/current/locked states, gate-latches between phases, and,
 * when Develop is current, the nested 5R dial + list for the active card.
 */
@Component({
  selector: 'lit-rhythm-rail',
  imports: [RouterLink, DialComponent, RListComponent],
  templateUrl: './rhythm-rail.component.html',
  styleUrl: './rhythm-rail.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RhythmRailComponent {
  readonly journey = input.required<ProjectJourney>();
  /** Logged 5R movements for the active Develop card; null hides the nested loop. */
  readonly fiveRFilled = input<number | null>(null);
  readonly signOut = output<void>();

  readonly phases = computed<Phase[]>(() =>
    [...this.journey().phases].sort((a, b) => a.order - b.order),
  );

  ordinal(kind: PhaseKind): string {
    return phaseOrdinal(kind);
  }

  sub(phase: Phase): string {
    switch (phase.state) {
      case 'Done':
        return 'Complete';
      case 'Current':
        return 'In progress';
      default:
        return 'Locked';
    }
  }

  /** The route a station links to, or null when it should not be navigable (locked). */
  linkFor(phase: Phase): unknown[] | null {
    const id = this.journey().id;
    switch (phase.kind) {
      case 'Discover':
        return ['/projects', id];
      case 'Discern':
        return phase.state === 'Locked' ? null : ['/discern', id];
      case 'Develop':
        return phase.state === 'Locked' ? null : ['/board', id];
      case 'Demonstrate':
        return phase.state === 'Locked' ? null : ['/demonstrate', id];
    }
  }

  latch(index: number): LatchState {
    const phase = this.phases()[index];
    if (phase?.gate?.state === 'Open') {
      return 'open';
    }
    if (phase?.gate?.state === 'Blocked' && phase.state !== 'Locked') {
      return 'blocked';
    }
    return 'locked';
  }

  latchLabel(index: number): string {
    switch (this.latch(index)) {
      case 'open':
        return 'Gate open';
      case 'blocked':
        return 'Gate blocked';
      default:
        return 'Locked';
    }
  }
}
